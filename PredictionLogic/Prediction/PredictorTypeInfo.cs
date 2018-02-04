using System;
using System.Collections.Generic;
using System.Reflection;
using PredictionLogic.Prediction.PredictorPropertyTypes;


namespace PredictionLogic.Prediction
{
    public class PredictorTypeInfo // : INotifyPropertyChanged
    {
        private Type predictorType;
        public Type PredictorType { get { return predictorType; } }

        private string displayName;
        public string DisplayName { get { return displayName; } }

        private string toolTipInfo;
        public string ToolTipInfo { get { return toolTipInfo; } }

        public bool IsChecked { get; set; }

        private List<PredictorPropertyBase> predictorProperties;
        public List<PredictorPropertyBase> PredictorProperties
        {
            get
            {
                return predictorProperties;
            }
        }

        public PredictorTypeInfo(Type predictorType, string displayName)
        {
            this.predictorType = predictorType;
            this.displayName = displayName;
            try
            {
                predictorProperties = (List<PredictorPropertyBase>)PredictorType.InvokeMember("Properties", BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public, null, null, null);
            }
            catch
            {
                ErrorNotifier.showError("Class \"" + predictorType.FullName + "\" is not implemented correctly.");
            }
            try
            {
                toolTipInfo = (string)PredictorType.InvokeMember("ToolTipInfo", BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public, null, null, null);
            }
            catch
            {
                toolTipInfo = displayName + " Branch Predictor";
            }
        }

        //public event PropertyChangedEventHandler PropertyChanged;

        //private static PropertyChangedEventArgs IsCheckedPropertyChangedEventArgs = new PropertyChangedEventArgs("IsChecked");

        //private bool isChecked;
        //public bool IsChecked
        //{
        //    get { return isChecked; }
        //    set
        //    {
        //        isChecked = value;
        //        if (PropertyChanged != null) PropertyChanged(this, IsCheckedPropertyChangedEventArgs);
        //    }
        //}
    }
}
