using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;

namespace PredictionLogic.Simulation
{
    [Serializable]
    public class SimulationOptions : INotifyPropertyChanged, ISerializable
    {
        private bool conditionalOnly = true;
        public bool ConditionalOnly
        {
            get
            {
                return conditionalOnly;
            }
            set
            {
                conditionalOnly = value;
                NotifyPropertyChanged(ConditionalOnlyPropertyChangedEventArgs);
            }
        }

        private int numberOfBranchesToSkip = 0;
        public int NumberOfBranchesToSkip
        {
            get
            {
                return numberOfBranchesToSkip;
            }
            set
            {
                numberOfBranchesToSkip = value;
                NotifyPropertyChanged(NumberOfBranchesToSkipPropertyChangedEventArgs);
            }
        }

        private bool remoteExecutionOnly = false;
        public bool RemoteExecutionOnly
        {
            get
            {
                return remoteExecutionOnly;
            }
            set
            {
                remoteExecutionOnly = value;
                NotifyPropertyChanged(RemoteExecutionOnlyPropertyChangedEventArgs);
            }
        }

        public SimulationOptions()
        {

        }

        public SimulationOptions GetCopy()
        {
            SimulationOptions copy = new SimulationOptions();
            copy.conditionalOnly = this.conditionalOnly;
            copy.numberOfBranchesToSkip = this.numberOfBranchesToSkip;
            copy.remoteExecutionOnly = this.remoteExecutionOnly;
            return copy;
        }

        #region Serialization

        public SimulationOptions(SerializationInfo info, StreamingContext context)
        {
            conditionalOnly = (bool)info.GetValue("conditionalOnly", typeof(bool));
            numberOfBranchesToSkip = (int)info.GetValue("nrBranchesToSkip", typeof(int));
            remoteExecutionOnly = false;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("conditionalOnly", conditionalOnly);
            info.AddValue("nrBranchesToSkip", numberOfBranchesToSkip);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;
        public static PropertyChangedEventArgs ConditionalOnlyPropertyChangedEventArgs = new PropertyChangedEventArgs("ConditionalOnly");
        public static PropertyChangedEventArgs NumberOfBranchesToSkipPropertyChangedEventArgs = new PropertyChangedEventArgs("NumberOfBranchesToSkip");
        public static PropertyChangedEventArgs RemoteExecutionOnlyPropertyChangedEventArgs = new PropertyChangedEventArgs("RemoteExecutionOnly");

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

        #endregion

        #region Static

        public static readonly SimulationOptions defaultOptions;

        static SimulationOptions()
        {
            defaultOptions = new SimulationOptions();
        }

        #endregion
    }
}
