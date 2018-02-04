using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PredictionLogic;

namespace BranchPredictionSimulatorServer
{
    public class ErrorNotifierActionServer : ErrorNotifierAction
    {
        public override void showError(string message)
        {
            Console.WriteLine("Error: " + message);
        }
    }
}
