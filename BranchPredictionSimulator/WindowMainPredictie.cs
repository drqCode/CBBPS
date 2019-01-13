﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;

using System.Collections.ObjectModel;
using PredictionLogic.Prediction;
using PredictionLogic.Prediction.BenchmarksAndReaders;
using PredictionLogic.Simulation;
using BranchPredictionSimulator.SimulationResultStructures;

namespace BranchPredictionSimulator {
	public partial class WindowMain {
		private List<TraceFileInfo> stanfordTraces = new List<TraceFileInfo>();

		public ObservableCollection<object> displayedResults = new ObservableCollection<object>();
		public Queue<SimulationInfo> simulationQueue = new Queue<SimulationInfo>();
		public SimulationResultsDictionary simulationResultsDictionary = new SimulationResultsDictionary();
		public SimulationSession session = null;

		public List<PredictorTypeInfo> predictorTypeList = new List<PredictorTypeInfo>();

		private void initTraces() {
			foreach (string filename in TraceFileInfo.stanfordFilenames) {
				stanfordTraces.Add(new TraceFileInfo { Filename = filename, Selected = false });
			}

			labelStanford.DataContext = stanfordTraces;
		}

		private void initPredictorTypeList() {
			predictorTypeList.Add(new PredictorTypeInfo(typeof(GAg), "GAg"));

			PredictorTypesListBox.DataContext = predictorTypeList;
			PredictorTypesListBox.SelectedIndex = 0;
		}

		private List<PredictorInfo> getPredictorRequests(PredictorTypeInfo predictorInfoType) {
			List<PredictorInfo> predictorInfo = new List<PredictorInfo>();

			// cartesian product for all valid and unique property values
			int propertyCount = predictorInfoType.PredictorProperties.Count;
			List<object>[] propertyValues = new List<object>[propertyCount];

			int index = 0;
			foreach (var property in predictorInfoType.PredictorProperties) {
				propertyValues[index] = property.loadValuesFromUI();
				index++;
			}

			int[] valueIndex = new int[propertyCount];
			for (index = 0; index < propertyCount; index++) {
				valueIndex[index] = -1;
			}

			index = 0;
			while (index >= 0) {
				valueIndex[index]++;

				if (valueIndex[index] == propertyValues[index].Count) {
					index--;  // backtrack
				}
				else {
					if (index == propertyCount - 1) // found a solution
					{
						object[] predictorArguments = new object[propertyCount];
						for (int k = 0; k < propertyCount; k++) {
							predictorArguments[k] = propertyValues[k][valueIndex[k]];
						}
						string description = predictorInfoType.DisplayName + " (" + predictorInfoType.PredictorProperties[0].DisplayName + " = " + propertyValues[0][valueIndex[0]].ToString();
						for (int k = 1; k < propertyCount; k++) {
							description += ", " + predictorInfoType.PredictorProperties[k].DisplayName + " = " + propertyValues[k][valueIndex[k]].ToString();
						}
						description += ")";

						predictorInfo.Add(new PredictorInfo(predictorInfoType.PredictorType, description, this.AppOptions, predictorArguments));
					}
					else {
						index++; // forward track
						valueIndex[index] = -1;
					}
				}
			}

			return predictorInfo;
		}

		private void simulate_Click(object sender, RoutedEventArgs e) {
			List<PredictorInfo> predictorRequests;

			if (!this.AppOptions.IsPredictorCompareMode) {
				PredictorTypeInfo predictorType = (PredictorTypeInfo)PredictorTypesListBox.SelectedItem;
				predictorRequests = getPredictorRequests(predictorType);
			}
			else {
				predictorRequests = new List<PredictorInfo>();
				foreach (PredictorTypeInfo predictorTypeInfo in predictorTypeList) {
					if (predictorTypeInfo.IsChecked) {
						predictorRequests.AddRange(getPredictorRequests(predictorTypeInfo));
					}
				}
			}

			// Stanford
			var stanfordTraceQuery = from trace in stanfordTraces
									 where trace.Selected
									 select trace.Filename;

			int numberOfSelectedTraces = stanfordTraceQuery.Count();
			if (numberOfSelectedTraces == 0) {
				displayedResults.Add(new ResultListMessage("(no traces selected)"));
				return;
			}
			if (predictorRequests.Count == 0) {
				displayedResults.Add(new ResultListMessage("(no predictors selected)"));
				return;
			}

			// start the simulation

			simulationResultsDictionary.Clear();
			simulationQueue.Clear();

			foreach (string traceFilename in stanfordTraceQuery) {
				simulationResultsDictionary.addBenchmark(new BenchmarkInfo(traceFilename));
			}

			foreach (PredictorInfo predictorRequest in predictorRequests) {
				foreach (BenchmarkInfo benchmarkInfo in simulationResultsDictionary.Benchmarks) {
					simulationQueue.Enqueue(new SimulationInfo(predictorRequest, benchmarkInfo));
				}
			}

			displayedResults.Add(new ResultListMessage("Starting " + simulationQueue.Count + " simulations on " + predictorRequests.Count + " predictor versions"));

			foreach (PredictorInfo predictorInfo in predictorRequests) {
				BenchmarkStatisticsCollection benchmarkStatisticsCollection = simulationResultsDictionary.addPredictor(predictorInfo);

				displayedResults.Add(new ResultListMessage("Simulation using " + predictorInfo.description));
				displayedResults.Add(benchmarkStatisticsCollection);
			}

			simulationResultsDictionary.initialize();

			// start worker threads
			this.startWork();
		}

		private void abort_Click(object sender, RoutedEventArgs e) {
			abortAllWork();
		}
	}
}
