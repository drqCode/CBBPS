using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using PredictionLogic;

namespace BranchPredictionSimulator
{
    public class ErrorNotifierActionClient : ErrorNotifierAction
    {
        public override void showError(string message)
        {
            MessageBox.Show(message, Localization.LanguageSelector.Global.getLocalizedString("Error"), MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
