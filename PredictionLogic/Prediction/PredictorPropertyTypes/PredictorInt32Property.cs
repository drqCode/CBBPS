using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace PredictionLogic.Prediction.PredictorPropertyTypes
{
    public class PredictorInt32Property : PredictorPropertyBase
    {
        public static readonly PropertyChangedEventArgs bindingStringPropertyChangedEventArgs = new PropertyChangedEventArgs("BindingString");
        public string BindingString { get; set; }

        public readonly int defaultValue;
        public readonly int minimumValue;
        public readonly int maximumValue;

        public PredictorInt32Property(string propertyName, string displayName, int defaultValue, int minimumValue, int maximumValue)
        {
            if ((defaultValue < minimumValue) || (defaultValue > maximumValue))
            {
                throw new ArgumentException("Error initializing property " + propertyName + ". Default value must be between the minimum and maximum.");
            }
            this.defaultValue = defaultValue;
            this.minimumValue = minimumValue;
            this.maximumValue = maximumValue;
            this.PropertyName = propertyName;
            this.DisplayName = displayName;
            this.BindingString = defaultValue.ToString();
        }

        public override List<object> loadValuesFromUI()
        {
            string corrected = "";
            List<object> outputValues = new List<object>();

            foreach (string s in BindingString.Split(','))
            {
                int value;
                if (int.TryParse(s, out value))
                {
                    if (value > this.maximumValue)
                    {
                        value = this.maximumValue;
                    }
                    if (value < this.minimumValue)
                    {
                        value = this.minimumValue;
                    }
                }
                else value = this.defaultValue;

                if (outputValues.IndexOf(value) == -1)
                {
                    if (corrected != "")
                    {
                        corrected += ", ";
                    }
                    corrected += value;
                    outputValues.Add(value);
                }
            }
            BindingString = corrected;
            NotifyPropertyChanged(bindingStringPropertyChangedEventArgs);
            return outputValues;
        }
    }
}
