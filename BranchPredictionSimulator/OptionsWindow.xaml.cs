using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using PredictionLogic;
using PredictionLogic.Prediction;
using PredictionLogic.Simulation;
using BranchPredictionSimulator.Localization;
using System.Windows.Forms;

namespace BranchPredictionSimulator
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();

            switch (LanguageSelector.Global.SelectedLanguage.identifier)
            {
                case "en":
                    cbiLangEnglish.IsSelected = true;
                    break;
                case "de":
                    cbiLangGerman.IsSelected = true;
                    break;
                case "ro":
                    cbiLangRomanian.IsSelected = true;
                    break;
            }

            NumNrBranchesToSkip.MinValue = 0;
            NumNrBranchesToSkip.MaxValue = int.MaxValue;
            NumNrBranchesToSkip.Increment = 1000;
        }

        private ApplicationOptionsClient applicationOptions;
        public ApplicationOptionsClient ApplicationOptions
        {
            get
            {
                return applicationOptions;
            }
            set
            {
                applicationOptions = value;
                this.GBAppOptions.DataContext = value;
            }
        }

        private SimulationOptions simulationOptions;
        public SimulationOptions SimulationOptions
        {
            get
            {
                return simulationOptions;
            }
            set
            {
                simulationOptions = value;
                this.GBSimulationOptions.DataContext = value;
            }
        }

        #region language selection

        private void cbiLangEnglish_Selected(object sender, RoutedEventArgs e)
        {
            LanguageSelector.Global.ChangeLanguage(LanguageInfo.English);
        }

        private void cbiLangRomanian_Selected(object sender, RoutedEventArgs e)
        {
            LanguageSelector.Global.ChangeLanguage(LanguageInfo.Romanian);
        }

        private void cbiLangGerman_Selected(object sender, RoutedEventArgs e)
        {
            LanguageSelector.Global.ChangeLanguage(LanguageInfo.German);
        }


        #endregion

        private static Brush textForegroundEnabled = new SolidColorBrush(Colors.Black);
        private static Brush textForegroundDisabled = new SolidColorBrush(Color.FromRgb(128, 128, 128));

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            ApplicationOptionsClient applicationOptions = this.GBAppOptions.DataContext as ApplicationOptionsClient;

            var result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                applicationOptions.TracePathMain = folderBrowserDialog.SelectedPath + "\\";
            }
        }

        private void tb_TracePathMain_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplicationOptionsClient appOptions = this.GBAppOptions.DataContext as ApplicationOptionsClient;
            appOptions.TracePathMain = tb_TracePathMain.Text;
        }
    }
}
