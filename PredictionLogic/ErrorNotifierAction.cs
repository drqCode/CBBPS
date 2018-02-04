using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PredictionLogic
{
    public abstract class ErrorNotifierAction
    {
        public abstract void showError(string message);
    }
}
