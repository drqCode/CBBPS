using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PredictionLogic.SimulationStatistics;
using PredictionLogic.Prediction;

namespace PredictionLogic.CommunicationProtocol
{
    [Serializable]
    public class ResultPackage
    {
        public uint taskID;
        public uint sessionID;
        public BenchmarkStatisticsResult result;

        [NonSerialized]
        public PredictorInfo predictor;
    }
}
