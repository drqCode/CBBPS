using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using PredictionLogic;
using PredictionLogic.Prediction;
using PredictionLogic.Simulation;

namespace GAgSimulator {
	public partial class WindowMain : Window {
		public ApplicationOptionsClient AppOptions { get; set; }
		public SimulationOptions SimulationOptionsForBinding { get; set; }
		public SimulationOptions SimulationOptionsCurrentExecution { get; set; }
		private OptionsWindow optionsWindow = null;

		public WindowMain() {
			// wire up error notifier action
			ErrorNotifier.errorNotifierAction = new ErrorNotifierActionClient();

			// init settings
			AppOptions = new ApplicationOptionsClient();
			SimulationOptionsForBinding = new SimulationOptions();

			InitializeComponent();

			initTraces();
			initPredictorTypeList();
			simulationResultsDictionary.filled += new EventHandler(simulationResultsDictionaryFilled);

			lbResults.DataContext = displayedResults;

			// the chart properties are not DependencyProperties so they have to be set programatically
			MainChart.ShowAM = AppOptions.ShowAM;
			MainChart.ShowLine = AppOptions.ShowLine;
			AppOptions.PropertyChanged += new PropertyChangedEventHandler(AppOptions_PropertyChanged);
		}

		void AppOptions_PropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == "ShowAM") {
				MainChart.ShowAM = AppOptions.ShowAM;
			}
			if (e.PropertyName == "ShowLine") {
				MainChart.ShowLine = AppOptions.ShowLine;
			}
		}

		#region Trace Selection ("Select All" / "Select None")

		private void selectAllStanford_Click(object sender, RoutedEventArgs e) {
			foreach (TraceFileInfo traceFileInfo in traces) {
				traceFileInfo.Selected = true;
			}
		}
		private void selectNoneStanford_Click(object sender, RoutedEventArgs e) {
			foreach (TraceFileInfo traceFileInfo in traces) {
				traceFileInfo.Selected = false;
			}
		}

		#endregion


		private void btnClearResults_Click(object sender, RoutedEventArgs e) {
			displayedResults.Clear();
		}

		private void MenuItemAbout_Click(object sender, RoutedEventArgs e) {
		}

		private void MenuItemExit_Click(object sender, RoutedEventArgs e) {
			killApplication();
		}

		private void RootWindow_Closing(object sender, CancelEventArgs e) {
			killApplication();
		}

		/// <summary>
		/// Tidies up and terminates the application.
		/// </summary>
		private void killApplication() {
			// abort any ongoing simulations
			abortAllWork();

			Application.Current.Shutdown();
		}

		private void TraceInfoDataTemplateDisplay_MouseDown(object sender, MouseButtonEventArgs e) {
			TraceFileInfo traceInfo = (sender as FrameworkElement).DataContext as TraceFileInfo;
			traceInfo.Selected = !traceInfo.Selected;
		}

		private void MenuItemOptions_Click(object sender, RoutedEventArgs e) {
			optionsWindow = new OptionsWindow();
			optionsWindow.ApplicationOptions = this.AppOptions;
			optionsWindow.SimulationOptions = this.SimulationOptionsForBinding;
			optionsWindow.Show();
		}
	}
}
