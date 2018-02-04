using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace PredictionLogic.Simulation
{
    [Serializable]
    public class SimulationSession : ISerializable
    {
        private static uint lastId = 0;
        private bool aborted = false;
        public bool Aborted { get { return aborted; } }

        public readonly uint sessionID;

        public readonly SimulationOptions sessionOptions;

        public SimulationSession(SimulationOptions options)
        {
            sessionID = ++lastId;
            sessionOptions = options;
        }

        #region Serialization

        public SimulationSession(SerializationInfo info, StreamingContext context)
        {
            sessionID = (uint)info.GetValue("SessionID", typeof(uint));
            sessionOptions = (SimulationOptions)info.GetValue("Options", typeof(SimulationOptions));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("SessionID", sessionID);
            info.AddValue("Options", sessionOptions);
        }

        #endregion
    }
}
