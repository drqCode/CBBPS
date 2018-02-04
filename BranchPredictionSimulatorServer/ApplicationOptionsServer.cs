using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Configuration;
using PredictionLogic;

namespace BranchPredictionSimulatorServer
{
    public class ApplicationOptionsServer : ApplicationOptions
    {
        public const string SERVER_SETTINGS_FILE_NAME = "settings_server.ini";

        public readonly List<int> listeningPorts = new List<int>();

        public ApplicationOptionsServer()
        {           
        }

        /// <summary>
        /// Loads the application configuration from settings_server.ini file and creates one if it doesn't exist
        /// </summary>
        /// <returns>Returns false whenever an error occurs.</returns>
        public bool loadOptions()
        {       
            try
            {
                PropertyFileHandler propertyFileHandler = new PropertyFileHandler(SERVER_SETTINGS_FILE_NAME);

                // path to benchmarks
                TracePathMain = propertyFileHandler.getPropertyValue("PathToBenchmarks", "");

                // ports
                string portsString = propertyFileHandler.getPropertyValue("ListeningPorts", "9050, 9051, 9052");

                string[] portsArray = portsString.Split(',');
                foreach (string portString in portsArray)
                {
                    int port;
                    if (int.TryParse(portString, out port))
                    {
                        listeningPorts.Add(port);
                    }
                }
                if (listeningPorts.Count == 0)
                {
                    ErrorNotifier.showError("No valid port numbers could be read from configuration file.");
                    return false;
                }

                propertyFileHandler.save(); // this will create the file if it didn't exist
                return true;
            }
            catch (Exception e)
            {
                ErrorNotifier.showError("Couldn't read options from disk: " + e.Message);
                return false;
            }
        }
    }
}
