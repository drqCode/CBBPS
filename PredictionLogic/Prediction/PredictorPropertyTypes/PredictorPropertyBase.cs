using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace PredictionLogic.Prediction.PredictorPropertyTypes {
	public abstract class PredictorPropertyBase : INotifyPropertyChanged {
		public string PropertyName { get; set; }
		public string DisplayName { get; set; }
		public abstract List<object> loadValuesFromUI();

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged(PropertyChangedEventArgs args) {
			PropertyChanged?.Invoke(this, args);
		}

		#endregion
	}
}
