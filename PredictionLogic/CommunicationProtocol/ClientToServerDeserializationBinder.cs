using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Reflection;

namespace PredictionLogic.CommunicationProtocol
{
    public class ClientToServerDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            string currentAssemblyName = Assembly.GetExecutingAssembly().FullName;
            Type typeToDeserialize = Type.GetType(String.Format("{0}, {1}", typeName, currentAssemblyName));
            return typeToDeserialize;
        }
    }
}
