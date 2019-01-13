using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PredictionLogic.Simulation;
using GAgSimulator.Localization;
using System.Windows.Forms;

namespace GAgSimulator {
	/// <summary>
	/// Interaction logic for OptionsWindow.xaml
	/// </summary>
	public partial class OptionsWindow : Window {
		public OptionsWindow() {
			InitializeComponent();
		}

		private ApplicationOptionsClient applicationOptions;
		public ApplicationOptionsClient ApplicationOptions {
			get {
				return applicationOptions;
			}
			set {
				applicationOptions = value;
				this.GBAppOptions.DataContext = value;
			}
		}

		private SimulationOptions simulationOptions;
		public SimulationOptions SimulationOptions {
			get {
				return simulationOptions;
			}
			set {
				simulationOptions = value;
			}
		}

		#region language selection

		private void cbiLangRomanian_Selected(object sender, RoutedEventArgs e) {
			LanguageSelector.Global.ChangeLanguage(LanguageInfo.Romanian);
		}

		#endregion

		private static Brush textForegroundEnabled = new SolidColorBrush(Colors.Black);
		private static Brush textForegroundDisabled = new SolidColorBrush(Color.FromRgb(128, 128, 128));

		private void btnBrowse_Click(object sender, RoutedEventArgs e) {
			FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
			ApplicationOptionsClient applicationOptions = this.GBAppOptions.DataContext as ApplicationOptionsClient;

			var result = folderBrowserDialog.ShowDialog();
			if (result == System.Windows.Forms.DialogResult.OK) {
				applicationOptions.TracePathMain = folderBrowserDialog.SelectedPath + "\\";
			}
		}

		private void tb_TracePathMain_TextChanged(object sender, TextChangedEventArgs e) {
			ApplicationOptionsClient appOptions = this.GBAppOptions.DataContext as ApplicationOptionsClient;
			appOptions.TracePathMain = tb_TracePathMain.Text;
		}
	}
}
