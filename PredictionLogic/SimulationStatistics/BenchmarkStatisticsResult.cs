using System;

namespace PredictionLogic.SimulationStatistics {
	[Serializable]
	public class BenchmarkStatisticsResult : StatisticsResult {
		public BenchmarkStatisticsResult(string benchmarkName)
			: base(benchmarkName) {
		}

		public int NumberOfBranches { get; set; }
		public int NumberOfCorrectPredictions { get; set; }
		public int NumberOfIncorrectPredictions { get; set; }
	}
}
