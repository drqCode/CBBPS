using System.IO;

namespace PredictionLogic {
	public class ApplicationOptions {
		public ApplicationOptions() {
		}

		#region trace paths

		protected string tracesPath = "";
		public virtual string TracePathMain {
			get {
				return tracesPath;
			}
			set {
				tracesPath = value;
			}
		}

		#endregion
	}
}
