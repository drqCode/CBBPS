using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace BranchPredictionSimulator.SimulationResultStructures
{
    public class ResultListMessage : INotifyPropertyChanged
    {
        private string message;
        public string Message
        {
            get
            {
                return message;
            }
        }

        private bool isVisible;
        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                isVisible = value;
                NotifyPropertyChanged(IsVisiblePropertyChangedEventArgs);
            }
        }

        public ResultListMessage(string message)
        {
            this.message = message;
            this.isVisible = true;
        }

        public ResultListMessage(string message, bool is_visible)
        {
            this.message = message;
            this.isVisible = is_visible;
        }

        //  INotifyPropertyChanged stuff

        public event PropertyChangedEventHandler PropertyChanged;
        public static PropertyChangedEventArgs IsVisiblePropertyChangedEventArgs = new PropertyChangedEventArgs("IsVisible");

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
