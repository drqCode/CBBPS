using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace PredictionLogic.Prediction.PredictorPropertyTypes {
	public class PredictorBoolProperty : PredictorPropertyBase {
		public static readonly PropertyChangedEventArgs bindingTakenPropertyChangedEventArgs = new PropertyChangedEventArgs("BindingTaken");
		public static readonly PropertyChangedEventArgs bindingNotTakenPropertyChangedEventArgs = new PropertyChangedEventArgs("BindingNotTaken");

		private bool bindingTaken;
		private bool bindingNotTaken;

		public bool BindingTaken {
			get {
				return bindingTaken;
			}
			set {
				bindingTaken = value;
				if (!bindingTaken && !bindingNotTaken) {
					bindingNotTaken = true;
					NotifyPropertyChanged(bindingNotTakenPropertyChangedEventArgs);
				}
			}
		}
		public bool BindingNotTaken {
			get {
				return bindingNotTaken;
			}
			set {
				bindingNotTaken = value;
				if (!bindingTaken && !bindingNotTaken) {
					bindingTaken = true;
					NotifyPropertyChanged(bindingTakenPropertyChangedEventArgs);
				}
			}
		}

		public string TextTrue { get; private set; }
		public string TextFalse { get; private set; }

		public PredictorBoolProperty(string propertyName, string displayName, string textTrue, string textFalse, bool defaultIsTrue) {
			this.PropertyName = propertyName;
			this.DisplayName = displayName;
			this.bindingTaken = defaultIsTrue;
			this.bindingNotTaken = !defaultIsTrue;
			this.TextTrue = textTrue;
			this.TextFalse = textFalse;
		}

		public override List<object> loadValuesFromUI() {
			List<object> outputValues = new List<object>();
			if (bindingTaken) {
				outputValues.Add(true);
			}
			if (bindingNotTaken) {
				outputValues.Add(false);
			}
			return outputValues;
		}
	}
}
