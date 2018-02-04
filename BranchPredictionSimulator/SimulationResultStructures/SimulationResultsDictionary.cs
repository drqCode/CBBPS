using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using PredictionLogic.Prediction;
using PredictionLogic.SimulationStatistics;
using PredictionLogic.Prediction.BenchmarksAndReaders;

namespace BranchPredictionSimulator.SimulationResultStructures
{
    public class SimulationResultsDictionary
    {
        private Dictionary<PredictorInfo, int> predictorIndexMap = new Dictionary<PredictorInfo, int>();
        private Dictionary<BenchmarkInfo, int> benchmarkIndexMap = new Dictionary<BenchmarkInfo, int>();
        private int numberOfPredictors = 0;
        private int numberOfBenchmarks = 0;
        private Dictionary<PredictorInfo, BenchmarkStatisticsCollection> observableCollections = new Dictionary<PredictorInfo, BenchmarkStatisticsCollection>();
        private BenchmarkStatisticsResult[,] resultMatrix = null;
        private int valuesEntered = 0;

        public event simulationResultsDictionaryNewValueReceived newValueReceived;

        public int ValuesEntered
        {
            get
            {
                return valuesEntered;
            }
        }
        public bool IsFull
        {
            get
            {
                return (resultMatrix != null) && (valuesEntered == numberOfPredictors * numberOfBenchmarks);
            }
        }
        public Dictionary<PredictorInfo, int>.KeyCollection Predictors
        {
            get
            {
                return predictorIndexMap.Keys;
            }
        }
        public Dictionary<BenchmarkInfo, int>.KeyCollection Benchmarks
        {
            get
            {
                return benchmarkIndexMap.Keys;
            }
        }
        public Dictionary<PredictorInfo, int> PredictorsWithIndices
        {
            get
            {
                return predictorIndexMap;
            }
        }
        public Dictionary<BenchmarkInfo, int> BenchmarksWithIndices
        {
            get
            {
                return benchmarkIndexMap;
            }
        }
        public int NrPredictors
        {
            get
            {
                return numberOfPredictors;
            }
        }
        public int NrBenchmarks
        {
            get
            {
                return numberOfBenchmarks;
            }
        }

        public event EventHandler filled;

        public BenchmarkStatisticsCollection addPredictor(PredictorInfo predictor)
        {
            try
            {
                predictorIndexMap.Add(predictor, numberOfPredictors++);
                BenchmarkStatisticsCollection benchmarkStatisticsCollection = new BenchmarkStatisticsCollection();
                observableCollections.Add(predictor, benchmarkStatisticsCollection);
                return benchmarkStatisticsCollection;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public void addBenchmark(BenchmarkInfo benchmark)
        {
            try
            {
                benchmarkIndexMap.Add(benchmark, numberOfBenchmarks++);
            }
            catch (Exception e)
            {
            }
        }

        public void initialize()
        {
            resultMatrix = new BenchmarkStatisticsResult[predictorIndexMap.Count, benchmarkIndexMap.Count];
        }

        public BenchmarkStatisticsResult getResult(PredictorInfo predictor, BenchmarkInfo benchmark)
        {
            int predictorIndex;
            int benchmarkIndex;
            if (!predictorIndexMap.TryGetValue(predictor, out predictorIndex))
            {
                throw new Exception("Invalid predictor provided.");
            }
            if (!benchmarkIndexMap.TryGetValue(benchmark, out benchmarkIndex))
            {
                throw new Exception("Invalid simulator provided.");
            }
            return resultMatrix[predictorIndex, benchmarkIndex];
        }

        public BenchmarkStatisticsResult getResult(int predictorIndex, int benchmarkIndex)
        {
            return resultMatrix[predictorIndex, benchmarkIndex];
        }

        public BenchmarkStatisticsCollection getResultCollectionForPredictor(PredictorInfo predictor)
        {
            return observableCollections[predictor];
        }

        public void setResult(PredictorInfo predictor, BenchmarkInfo benchmark, BenchmarkStatisticsResult value)
        {
            int predictorIndex;
            int benchmarkIndex;
            if (!predictorIndexMap.TryGetValue(predictor, out predictorIndex))
            {
                throw new Exception("Invalid predictor provided.");
            }
            if (!benchmarkIndexMap.TryGetValue(benchmark, out benchmarkIndex))
            {
                throw new Exception("Invalid simulator provided.");
            }
            if ((value != null) && (resultMatrix[predictorIndex, benchmarkIndex] == null))
            {
                valuesEntered++;
                observableCollections[predictor].addSorted(value);
                if (newValueReceived != null)
                {
                    newValueReceived(this, new SimulationResultsDictionaryNewValueReceivedEventArgs(predictorIndex, benchmarkIndex, value, observableCollections[predictor]));
                }
            }
            else
            {
                throw new NotSupportedException();
            }

            resultMatrix[predictorIndex, benchmarkIndex] = value;

            if ((valuesEntered == numberOfPredictors * numberOfBenchmarks) && (filled != null))
            {
                filled(this, EventArgs.Empty);
            }
        }

        public void setResultUsingDispatcher(Dispatcher dispatcher, PredictorInfo predictor, BenchmarkInfo benchmark, BenchmarkStatisticsResult value)
        {
            int predictorIndex;
            int benchmarkIndex;
            if (!predictorIndexMap.TryGetValue(predictor, out predictorIndex))
            {
                throw new Exception("Invalid predictor provided.");
            }
            if (!benchmarkIndexMap.TryGetValue(benchmark, out benchmarkIndex))
            {
                throw new Exception("Invalid simulator provided.");
            }
            if ((value != null) && (resultMatrix[predictorIndex, benchmarkIndex] == null))
            {
                valuesEntered++;
                dispatcher.BeginInvoke(DispatcherPriority.Normal,
                  new Action(
                    delegate()
                    {
                        observableCollections[predictor].addSorted(value);
                        if (newValueReceived != null)
                        {
                            newValueReceived(this, new SimulationResultsDictionaryNewValueReceivedEventArgs(predictorIndex, benchmarkIndex, value, observableCollections[predictor]));
                        }
                    }
                ));
            }
            else
            {
                throw new NotSupportedException();
            }

            resultMatrix[predictorIndex, benchmarkIndex] = value;
            if ((valuesEntered == numberOfPredictors * numberOfBenchmarks) && (filled != null))
            {
                filled(this, EventArgs.Empty);
            }
        }

        public void Clear()
        {
            predictorIndexMap.Clear();
            benchmarkIndexMap.Clear();
            numberOfPredictors = 0;
            numberOfBenchmarks = 0;
            observableCollections.Clear();
            resultMatrix = null;
            valuesEntered = 0;
        }
    }

    public delegate void simulationResultsDictionaryNewValueReceived(object sender, SimulationResultsDictionaryNewValueReceivedEventArgs e);

    public class SimulationResultsDictionaryNewValueReceivedEventArgs : EventArgs
    {
        public readonly int predictorIndex;
        public readonly int benchmarkIndex;
        public readonly BenchmarkStatisticsResult value;
        public readonly BenchmarkStatisticsCollection correspondingCollection;

        public SimulationResultsDictionaryNewValueReceivedEventArgs(int predictorIndex, int benchmarkIndex, BenchmarkStatisticsResult value, BenchmarkStatisticsCollection collection)
        {
            this.predictorIndex = predictorIndex;
            this.benchmarkIndex = benchmarkIndex;
            this.value = value;
            this.correspondingCollection = collection;
        }
    }
}
