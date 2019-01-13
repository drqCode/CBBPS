using System.ComponentModel;
using PredictionLogic;

namespace GAgSimulator {
	public class ApplicationOptionsClient : ApplicationOptions, INotifyPropertyChanged {
		public override string TracePathMain {
			get {
				return base.TracePathMain;
			}
			set {
				base.TracePathMain = value;
				NotifyPropertyChanged(TracePathMainPropertyChangedEventArgs);
			}
		}

		#region chart options

		private bool showAM = true;
		public bool ShowAM {
			get {
				return showAM;
			}
			set {
				showAM = value;
				NotifyPropertyChanged(ShowAMPropertyChangedEventArgs);
			}
		}

		private bool showLine = false;
		public bool ShowLine {
			get {
				return showLine;
			}
			set {
				showLine = value;
				NotifyPropertyChanged(ShowLinePropertyChangedEventArgs);
			}
		}

		#endregion

		public ApplicationOptionsClient() {
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		public static PropertyChangedEventArgs ShowAMPropertyChangedEventArgs = new PropertyChangedEventArgs("ShowAM");
		public static PropertyChangedEventArgs ShowLinePropertyChangedEventArgs = new PropertyChangedEventArgs("ShowLine");

		public static PropertyChangedEventArgs TracePathMainPropertyChangedEventArgs = new PropertyChangedEventArgs("TracePathMain");

		public void NotifyPropertyChanged(string propertyName) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void NotifyPropertyChanged(PropertyChangedEventArgs propertyChangedEventArgs) {
			PropertyChanged?.Invoke(this, propertyChangedEventArgs);
		}

		#endregion

	}
}
