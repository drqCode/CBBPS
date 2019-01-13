using System.ComponentModel;

namespace PredictionLogic.Prediction {
	public class TraceFileInfo : INotifyPropertyChanged {
		public event PropertyChangedEventHandler PropertyChanged;

		private static PropertyChangedEventArgs filenamePropertyChangedEventArgs = new PropertyChangedEventArgs("Filename");
		private static PropertyChangedEventArgs selectedPropertyChangedEventArgs = new PropertyChangedEventArgs("Selected");

		private string filename;
		public string Filename {
			get {
				return filename;
			}
			set {
				filename = value;
				PropertyChanged?.Invoke(this, filenamePropertyChangedEventArgs);
			}
		}

		private bool selected;
		public bool Selected {
			get {
				return selected;
			}
			set {
				selected = value;
				PropertyChanged?.Invoke(this, selectedPropertyChangedEventArgs);
			}
		}

		public static string[] stanfordFilenames;
		public static string[] cbp2Filenames;
		public static string[] spec2000Filenames;

		static TraceFileInfo() {
			stanfordFilenames = new string[8];
			stanfordFilenames[0] = "fbubble.tra";
			stanfordFilenames[1] = "fmatrix.tra";
			stanfordFilenames[2] = "fperm.tra";
			stanfordFilenames[3] = "fpuzzle.tra";
			stanfordFilenames[4] = "fqueens.tra";
			stanfordFilenames[5] = "fsort.tra";
			stanfordFilenames[6] = "ftower.tra";
			stanfordFilenames[7] = "ftree.tra";
		}
	}
}
