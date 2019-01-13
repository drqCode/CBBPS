using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PredictionLogic.Prediction.BenchmarksAndReaders {
	[Serializable]
	public class BenchmarkInfo {
		public readonly string benchmarkName;

		public BenchmarkInfo(string benchmarkName) {
			this.benchmarkName = benchmarkName;
		}

		public override bool Equals(object obj) {
			if (obj == null || GetType() != obj.GetType()) {
				return false;
			}

			BenchmarkInfo other = obj as BenchmarkInfo;
			return (benchmarkName.Equals(other.benchmarkName));
		}

		public override int GetHashCode() {
			return benchmarkName.GetHashCode();
		}
	}
}
