using System.Collections.ObjectModel;
using PredictionLogic.SimulationStatistics;
using System.Collections.Specialized;

namespace GAgSimulator.SimulationResultStructures {
	public class BenchmarkStatisticsCollection : ObservableCollection<BenchmarkStatisticsResult> {
		public StatisticsResult ArithmeticMean { get; set; }
		public ResultListMessage FinalisationMessage { get; set; }
		public bool ShowFinalisationMessage { get; set; }

		public BenchmarkStatisticsCollection() {
			this.CollectionChanged += new NotifyCollectionChangedEventHandler(benchmarkStatisticsCollectionCollectionChanged);
			ArithmeticMean = new StatisticsResult("Medie");
			FinalisationMessage = new ResultListMessage("Terminat", false);
		}

		void benchmarkStatisticsCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
			calculateMeans();
			if (ShowFinalisationMessage) {
				FinalisationMessage.IsVisible = true;
			}
			else FinalisationMessage.IsVisible = false;
		}

		public void addSorted(BenchmarkStatisticsResult item) {
			int i = 0;
			foreach (BenchmarkStatisticsResult benchmarkStatisticsResult in this) {
				if (benchmarkStatisticsResult.Name.CompareTo(item.Name) > 0) {
					break;
				}
				i++;
			}
			this.InsertItem(i, item);
		}

		public void calculateMeans() {
			double sum = 0;
			int numberOfResults = 0;
			foreach (BenchmarkStatisticsResult benchmarkStatisticsResult in this)
				if (benchmarkStatisticsResult.Accuracy != double.NaN && benchmarkStatisticsResult.Accuracy > 0) {
					sum += benchmarkStatisticsResult.Accuracy;
					numberOfResults++;
				}
			if (numberOfResults != 0) {
				ArithmeticMean.Accuracy = sum / numberOfResults;
			}
			else {
				ArithmeticMean.Accuracy = 0;
			}

			ArithmeticMean.NotifyPropertyChanged(StatisticsResult.PercentAccuracyPropertyChangedEventArgs);
		}
	}
}
