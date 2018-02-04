using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO;
using System.Configuration;
using PredictionLogic;

namespace BranchPredictionSimulator
{
    public class ApplicationOptionsClient : ApplicationOptions, INotifyPropertyChanged
    {
        private bool isPredictorCompareMode = false;
        public bool IsPredictorCompareMode
        {
            get
            {
                return isPredictorCompareMode;
            }
            set
            {
                isPredictorCompareMode = value;
                NotifyPropertyChanged(IsPredictorCompareModePropertyChangedEventArgs);
            }
        }

        public override string TracePathMain
        {
            get
            {
                return base.TracePathMain;
            }
            set
            {
                base.TracePathMain = value;
                NotifyPropertyChanged(TracePathMainPropertyChangedEventArgs);
                NotifyPropertyChanged(TracePathSpec2000PropertyChangedEventArgs);
                NotifyPropertyChanged(TracePathCBP2PropertyChangedEventArgs);
                NotifyPropertyChanged(TracePathStanfordPropertyChangedEventArgs);
            }
        }

        #region chart options

        private bool showAM = true;
        public bool ShowAM
        {
            get
            {
                return showAM;
            }
            set
            {
                showAM = value;
                NotifyPropertyChanged(ShowAMPropertyChangedEventArgs);
            }
        }

        private bool showGM = false;
        public bool ShowGM
        {
            get
            {
                return showGM;
            }
            set
            {
                showGM = value;
                NotifyPropertyChanged(ShowGMPropertyChangedEventArgs);
            }
        }

        private bool showHM = false;
        public bool ShowHM
        {
            get
            {
                return showHM;
            }
            set
            {
                showHM = value;
                NotifyPropertyChanged(ShowHMPropertyChangedEventArgs);
            }
        }

        private bool showLine = false;
        public bool ShowLine
        {
            get
            {
                return showLine;
            }
            set
            {
                showLine = value;
                NotifyPropertyChanged(ShowLinePropertyChangedEventArgs);
            }
        }

        #endregion

        public ApplicationOptionsClient()
        {
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public static PropertyChangedEventArgs IsPredictorCompareModePropertyChangedEventArgs = new PropertyChangedEventArgs("IsPredictorCompareMode");
        public static PropertyChangedEventArgs ShowAMPropertyChangedEventArgs = new PropertyChangedEventArgs("ShowAM");
        public static PropertyChangedEventArgs ShowGMPropertyChangedEventArgs = new PropertyChangedEventArgs("ShowGM");
        public static PropertyChangedEventArgs ShowHMPropertyChangedEventArgs = new PropertyChangedEventArgs("ShowHM");
        public static PropertyChangedEventArgs ShowLinePropertyChangedEventArgs = new PropertyChangedEventArgs("ShowLine");

        public static PropertyChangedEventArgs TracePathMainPropertyChangedEventArgs = new PropertyChangedEventArgs("TracePathMain");
        public static PropertyChangedEventArgs TracePathSpec2000PropertyChangedEventArgs = new PropertyChangedEventArgs("TracePathSpec2000");
        public static PropertyChangedEventArgs TracePathCBP2PropertyChangedEventArgs = new PropertyChangedEventArgs("TracePathCBP2");
        public static PropertyChangedEventArgs TracePathStanfordPropertyChangedEventArgs = new PropertyChangedEventArgs("TracePathStanford");

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

    }
}
