using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using PredictionLogic.SimulationStatistics;
using System.Collections.Specialized;

namespace BranchPredictionSimulator.SimulationResultStructures
{
    public class BenchmarkStatisticsCollection : ObservableCollection<BenchmarkStatisticsResult>
    {
        public StatisticsResult ArithmeticMean { get; set; }
        public StatisticsResult GeometricMean { get; set; }
        public StatisticsResult HarmonicMean { get; set; }
        public ResultListMessage FinalisationMessage { get; set; }
        public bool ShowFinalisationMessage { get; set; }

        public BenchmarkStatisticsCollection()
        {
            this.CollectionChanged += new NotifyCollectionChangedEventHandler(benchmarkStatisticsCollectionCollectionChanged);
            ArithmeticMean = new StatisticsResult("Arithmetic Mean");
            GeometricMean = new StatisticsResult("Geometric Mean");
            HarmonicMean = new StatisticsResult("Harmonic Mean");
            FinalisationMessage = new ResultListMessage("Finished", false);
        }

        void benchmarkStatisticsCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            calculateMeans();
            if (ShowFinalisationMessage)
            {
                FinalisationMessage.IsVisible = true;
            }
            else FinalisationMessage.IsVisible = false;
        }

        public void addSorted(BenchmarkStatisticsResult item)
        {
            int i = 0;
            foreach (BenchmarkStatisticsResult benchmarkStatisticsResult in this)
            {
                if (benchmarkStatisticsResult.Name.CompareTo(item.Name) > 0)
                {
                    break;
                }
                i++;
            }
            this.InsertItem(i, item);
        }

        public void calculateMeans()
        {
            double sum = 0;
            double inverseSum = 0;
            double product = 1;
            int numberOfResults = 0;
            double arithmeticalSum = 0;
            double arithmeticalTotal = 0;
            foreach (BenchmarkStatisticsResult benchmarkStatisticsResult in this)
                if (benchmarkStatisticsResult.Accuracy != double.NaN && benchmarkStatisticsResult.Accuracy > 0)
                {
                    arithmeticalTotal += benchmarkStatisticsResult.NumberOfBranches;
                    arithmeticalSum += benchmarkStatisticsResult.NumberOfCorrectPredictions;
                    sum += benchmarkStatisticsResult.Accuracy;
                    inverseSum += 1.0d / benchmarkStatisticsResult.Accuracy;
                    product *= benchmarkStatisticsResult.Accuracy;
                    numberOfResults++;
                }
            if (numberOfResults != 0)
            {
                ArithmeticMean.Accuracy = sum / numberOfResults;
                GeometricMean.Accuracy = Math.Pow(product, 1.0d / numberOfResults);
                HarmonicMean.Accuracy = numberOfResults / inverseSum;
            }
            else
            {
                ArithmeticMean.Accuracy = 0;
                GeometricMean.Accuracy = 0;
                HarmonicMean.Accuracy = 0;
            }

            ArithmeticMean.NotifyPropertyChanged(StatisticsResult.PercentAccuracyPropertyChangedEventArgs);
            GeometricMean.NotifyPropertyChanged(StatisticsResult.PercentAccuracyPropertyChangedEventArgs);
            HarmonicMean.NotifyPropertyChanged(StatisticsResult.PercentAccuracyPropertyChangedEventArgs);
        }
    }
}
