using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections.ObjectModel;
using PredictionLogic;
using PredictionLogic.Prediction;
using PredictionLogic.Prediction.Predictors;
using PredictionLogic.Prediction.BenchmarksAndReaders;
using PredictionLogic.Simulation;
using BranchPredictionSimulator.SimulationResultStructures;

namespace BranchPredictionSimulator
{
    public partial class WindowMain
    {
        private List<TraceFileInfo> stanfordTraces = new List<TraceFileInfo>();
        private List<TraceFileInfo> cbp2Traces = new List<TraceFileInfo>();
        private List<TraceFileInfo> spec2000Traces = new List<TraceFileInfo>();

        public ObservableCollection<object> displayedResults = new ObservableCollection<object>();
        public Queue<SimulationInfo> simulationQueue = new Queue<SimulationInfo>();
        public SimulationResultsDictionary simulationResultsDictionary = new SimulationResultsDictionary();
        public SimulationSession session = null;

        public List<PredictorTypeInfo> predictorTypeList = new List<PredictorTypeInfo>();

        private void initTraces()
        {
            foreach (string filename in TraceFileInfo.stanfordFilenames)
            {
                stanfordTraces.Add(new TraceFileInfo { Filename = filename, Selected = false });
            }
            foreach (string filename in TraceFileInfo.cbp2Filenames)
            {
                cbp2Traces.Add(new TraceFileInfo { Filename = filename, Selected = false });
            }
            foreach (string filename in TraceFileInfo.spec2000Filenames)
            {
                spec2000Traces.Add(new TraceFileInfo { Filename = filename, Selected = false });
            }

            labelStanford.DataContext = stanfordTraces;
            labelCbp2.DataContext = cbp2Traces;
            labelSpec2000.DataContext = spec2000Traces;
        }

        private void initPredictorTypeList()
        {
            predictorTypeList.Add(new PredictorTypeInfo(typeof(Perceptron), "Perceptron"));
            predictorTypeList.Add(new PredictorTypeInfo(typeof(FPBNP), "FPBNP"));
            predictorTypeList.Add(new PredictorTypeInfo(typeof(Piecewise), "Piecewise"));
            predictorTypeList.Add(new PredictorTypeInfo(typeof(OGEHL), "O-GEHL"));
            predictorTypeList.Add(new PredictorTypeInfo(typeof(GShare), "GShare"));
            predictorTypeList.Add(new PredictorTypeInfo(typeof(LGShare), "LGShare"));
            predictorTypeList.Add(new PredictorTypeInfo(typeof(GAg), "GAg"));
            predictorTypeList.Add(new PredictorTypeInfo(typeof(PAg), "PAg"));
            predictorTypeList.Add(new PredictorTypeInfo(typeof(PAp), "PAp"));
            predictorTypeList.Add(new PredictorTypeInfo(typeof(SIP1), "SIP1"));
            predictorTypeList.Add(new PredictorTypeInfo(typeof(SIP2), "SIP2"));
            predictorTypeList.Add(new PredictorTypeInfo(typeof(SIP3), "SIP3"));
            predictorTypeList.Add(new PredictorTypeInfo(typeof(FixedPredictor), "Fixed Prediction"));

            PredictorTypesListBox.DataContext = predictorTypeList;
            PredictorTypesListBox.SelectedIndex = 0;
        }

        private List<PredictorInfo> getPredictorRequests(PredictorTypeInfo predictorInfoType)
        {
            List<PredictorInfo> predictorInfo = new List<PredictorInfo>();

            // cartesian product for all valid and unique property values
            int propertyCount = predictorInfoType.PredictorProperties.Count;
            List<object>[] propertyValues = new List<object>[propertyCount];

            int index = 0;
            foreach (var property in predictorInfoType.PredictorProperties)
            {
                propertyValues[index] = property.loadValuesFromUI();
                index++;
            }

            int[] valueIndex = new int[propertyCount];
            for (index = 0; index < propertyCount; index++)
            {
                valueIndex[index] = -1;
            }

            index = 0;
            while (index >= 0)
            {
                valueIndex[index]++;

                if (valueIndex[index] == propertyValues[index].Count)
                {
                    index--;  // backtrack
                }
                else
                {
                    if (index == propertyCount - 1) // found a solution
                    {
                        object[] predictorArguments = new object[propertyCount];
                        for (int k = 0; k < propertyCount; k++)
                        {
                            predictorArguments[k] = propertyValues[k][valueIndex[k]];
                        }
                        string description = predictorInfoType.DisplayName + " (" + predictorInfoType.PredictorProperties[0].DisplayName + " = " + propertyValues[0][valueIndex[0]].ToString();
                        for (int k = 1; k < propertyCount; k++)
                        {
                            description += ", " + predictorInfoType.PredictorProperties[k].DisplayName + " = " + propertyValues[k][valueIndex[k]].ToString();
                        }
                        description += ")";

                        predictorInfo.Add(new PredictorInfo(predictorInfoType.PredictorType, description, this.AppOptions, predictorArguments));
                    }
                    else
                    {
                        index++; // forward track
                        valueIndex[index] = -1;
                    }
                }
            }

            return predictorInfo;
        }

        private void simulate_Click(object sender, RoutedEventArgs e)
        {
            List<PredictorInfo> predictorRequests;

            if (!this.AppOptions.IsPredictorCompareMode)
            {
                PredictorTypeInfo predictorType = (PredictorTypeInfo)PredictorTypesListBox.SelectedItem;
                predictorRequests = getPredictorRequests(predictorType);
            }
            else
            {
                predictorRequests = new List<PredictorInfo>();
                foreach (PredictorTypeInfo predictorTypeInfo in predictorTypeList)
                {
                    if (predictorTypeInfo.IsChecked)
                    {
                        predictorRequests.AddRange(getPredictorRequests(predictorTypeInfo));
                    }
                }
            }

            // Stanford
            var stanfordTraceQuery = from trace in stanfordTraces
                                     where trace.Selected
                                     select trace.Filename;

            // SPEC2000
            var spec2000TraceQuery = from trace in spec2000Traces
                                     where trace.Selected
                                     select trace.Filename;

            // CBP 2
            var CBP2TraceQuery = from trace in cbp2Traces
                                 where trace.Selected
                                 select trace.Filename;

            int numberOfSelectedTraces = stanfordTraceQuery.Count() + spec2000TraceQuery.Count() + CBP2TraceQuery.Count();
            if (numberOfSelectedTraces == 0)
            {
                displayedResults.Add(new ResultListMessage("(no traces selected)"));
                return;
            }
            if (predictorRequests.Count == 0)
            {
                displayedResults.Add(new ResultListMessage("(no predictors selected)"));
                return;
            }

            // start the simulation

            simulationResultsDictionary.Clear();
            simulationQueue.Clear();

            foreach (string traceFilename in stanfordTraceQuery)
            {
                simulationResultsDictionary.addBenchmark(new BenchmarkInfo(traceFilename, BenchmarkType.Stanford));
            }
            foreach (string traceFilename in spec2000TraceQuery)
            {
                simulationResultsDictionary.addBenchmark(new BenchmarkInfo(traceFilename, BenchmarkType.SPEC2000));
            }
            foreach (string traceFilename in CBP2TraceQuery)
            {
                simulationResultsDictionary.addBenchmark(new BenchmarkInfo(traceFilename, BenchmarkType.CBP2));
            }

            foreach (PredictorInfo predictorRequest in predictorRequests)
            {
                foreach (BenchmarkInfo benchmarkInfo in simulationResultsDictionary.Benchmarks)
                {
                    simulationQueue.Enqueue(new SimulationInfo(predictorRequest, benchmarkInfo));
                }
            }

            displayedResults.Add(new ResultListMessage("Starting " + simulationQueue.Count + " simulations on " + predictorRequests.Count + " predictor versions"));

            foreach (PredictorInfo predictorInfo in predictorRequests)
            {
                BenchmarkStatisticsCollection benchmarkStatisticsCollection = simulationResultsDictionary.addPredictor(predictorInfo);

                displayedResults.Add(new ResultListMessage("Simulation using " + predictorInfo.description));
                displayedResults.Add(benchmarkStatisticsCollection);
            }

            simulationResultsDictionary.initialize();

            // start worker threads
            this.startWork();
        }

        private void abort_Click(object sender, RoutedEventArgs e)
        {
            abortAllWork();
        }
    }
}
