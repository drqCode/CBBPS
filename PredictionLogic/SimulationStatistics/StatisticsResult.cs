using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace PredictionLogic.SimulationStatistics
{
    [Serializable]
    public class StatisticsResult : INotifyPropertyChanged
    {
        private string name;

        public StatisticsResult(string name)
        {
            this.name = name;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public double Accuracy { get; set; }

        public string PercentAccuracy
        {
            get
            {
                return double.IsNaN(Accuracy) ? "N/A" : ("" + Accuracy * 100 + " %");
            }
        }
        
        // INotifyPropertyChanged stuff

        public event PropertyChangedEventHandler PropertyChanged;
        public static PropertyChangedEventArgs AccuracyPropertyChangedEventArgs = new PropertyChangedEventArgs("Accuracy");
        public static PropertyChangedEventArgs PercentAccuracyPropertyChangedEventArgs = new PropertyChangedEventArgs("PercentAccuracy");

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void NotifyPropertyChanged(PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, propertyChangedEventArgs);
            }
        }
    }
}
