using System;
using PredictionLogic.Prediction.BenchmarksAndReaders;
using PredictionLogic.SimulationStatistics;
using PredictionLogic.Prediction;

namespace PredictionLogic.Simulation {
	[Serializable]
	public class SimulationInfo {
		public readonly PredictorInfo predictorInfo;
		public readonly BenchmarkInfo benchmarkInfo;

		public SimulationInfo(PredictorInfo predictorInfo, BenchmarkInfo benchmarkInfo) {
			this.predictorInfo = predictorInfo;
			this.benchmarkInfo = benchmarkInfo;
		}

		public SimulationInfo(PredictorInfo predictorInfo, string benchmarkName) {
			this.predictorInfo = predictorInfo;
			this.benchmarkInfo = new BenchmarkInfo(benchmarkName);
		}

		public BenchmarkStatisticsResult simulate(SimulationOptions simulationOptions, ApplicationOptions applicationOptions) {
			return Simulator.Call(predictorInfo, benchmarkInfo, simulationOptions, applicationOptions);
		}
	}
}
