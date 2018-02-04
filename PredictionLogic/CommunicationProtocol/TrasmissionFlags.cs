using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PredictionLogic.CommunicationProtocol
{
    public enum TrasmissionFlags
    {
        ClientName = 1,   // followed by machine name (string)
        TaskRequest = 2,  // followed by the session ID (uint)
        Task = 3,         // followed by Task Package (TaskPackage)
        Result = 4,       // followed by Result Package (ResultPackage)
        NewSession = 5,   // followed by the new session ID (uint)
        AbortSession = 6  // followed by the session ID (uint)
    }
}
