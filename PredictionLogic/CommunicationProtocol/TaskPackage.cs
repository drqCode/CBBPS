using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PredictionLogic.Prediction;
using PredictionLogic.Simulation;

namespace PredictionLogic.CommunicationProtocol
{
    [Serializable]
    public class TaskPackage
    {
        public uint taskID;
        public uint sessionID;
        public SimulationInfo simulationTask;
    }
}
