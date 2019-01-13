using System;
using System.ComponentModel;

namespace PredictionLogic.Simulation {
	[Serializable]
	public class SimulationOptions : INotifyPropertyChanged {
		public SimulationOptions() {

		}

		public SimulationOptions GetCopy() {
			return new SimulationOptions();
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		public void NotifyPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void NotifyPropertyChanged(PropertyChangedEventArgs propertyChangedEventArgs) {
			PropertyChanged?.Invoke(this, propertyChangedEventArgs);
		}

		#endregion

		#region Static

		public static readonly SimulationOptions defaultOptions;

		static SimulationOptions() {
			defaultOptions = new SimulationOptions();
		}

		#endregion
	}
}
