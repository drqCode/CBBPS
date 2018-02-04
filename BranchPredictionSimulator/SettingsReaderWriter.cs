using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using PredictionLogic;
using PredictionLogic.Prediction;
using PredictionLogic.Simulation;

namespace BranchPredictionSimulator
{
    static class SettingsReaderWriter
    {
        public const string APP_SETTINGS_FILE_NAME = "settings_app.xml";
        public const string SIM_SETTINGS_FILE_NAME = "settings_sim.ini";

        private const string XML_TAG_ROOT = "AppSettings";

        private const string XML_TAG_LANGUAGE = "Language";
        private const string XML_ATTR_LANGUAGE_ID = "Id";

        private const string XML_TAG_CONNECTIONS = "Connections";
        private const string XML_TAG_CONNECTION = "Connection";
        private const string XML_ATTR_CONNECTION_HOSTNAME = "Hostname";
        private const string XML_ATTR_CONNECTION_PORT = "Port";

        private const string XML_TAG_TRACE_PATH_MAIN = "PathToBenchmarks";
        private const string XML_TAG_IsPredictorComparison = "IsPredictorComparisonMode";
        private const string XML_TAG_SHOW_AM = "ShowArithmeticMean";
        private const string XML_TAG_SHOW_GM = "ShowGeometricMean";
        private const string XML_TAG_SHOW_HM = "ShowHarmonicMean";
        private const string XML_TAG_SHOW_LINE = "ShowLine";
        private const string XML_ATTR_VALUE = "Value";

        private static IFormatter formatter = new SoapFormatter();

        public static void loadApplicationSettingsFromDisk(ApplicationOptionsClient applicationOptions, WindowMain connectionHolder)
        {
            if (!File.Exists(APP_SETTINGS_FILE_NAME))
            {
                return;
            }

            XmlTextReader textReader = null;

            try
            {
                textReader = new XmlTextReader(APP_SETTINGS_FILE_NAME);
                textReader.Read(); // read declaration

                // read root node
                do
                {
                    textReader.Read();
                }
                while (textReader.NodeType == XmlNodeType.Whitespace);

                if (textReader.NodeType != XmlNodeType.Element || textReader.Name != XML_TAG_ROOT)
                {
                    throw new Exception("Error processing xml metadata file. The file might be corrupted.");
                }

                while (textReader.Read())
                {
                    if (textReader.NodeType == XmlNodeType.Element)
                    {
                        switch (textReader.Name)
                        {
                            case XML_TAG_LANGUAGE:
                                string lang_id = textReader.GetAttribute(XML_ATTR_LANGUAGE_ID);
                                Localization.LanguageSelector.Global.ChangeLanguage(Localization.LanguageInfo.getLanguageInfo(lang_id));
                                break;

                            case XML_TAG_IsPredictorComparison:
                                applicationOptions.IsPredictorCompareMode = bool.Parse(textReader.GetAttribute(XML_ATTR_VALUE));
                                break;

                            case XML_TAG_TRACE_PATH_MAIN:
                                applicationOptions.TracePathMain = textReader.GetAttribute(XML_ATTR_VALUE);
                                break;

                            case XML_TAG_SHOW_AM:
                                applicationOptions.ShowAM = bool.Parse(textReader.GetAttribute(XML_ATTR_VALUE));
                                break;

                            case XML_TAG_SHOW_GM:
                                applicationOptions.ShowGM = bool.Parse(textReader.GetAttribute(XML_ATTR_VALUE));
                                break;

                            case XML_TAG_SHOW_HM:
                                applicationOptions.ShowHM = bool.Parse(textReader.GetAttribute(XML_ATTR_VALUE));
                                break;

                            case XML_TAG_SHOW_LINE:
                                applicationOptions.ShowLine = bool.Parse(textReader.GetAttribute(XML_ATTR_VALUE));
                                break;

                            case XML_TAG_CONNECTIONS:
                                break;

                            case XML_TAG_CONNECTION:
                                string host = textReader.GetAttribute(XML_ATTR_CONNECTION_HOSTNAME);
                                string port = textReader.GetAttribute(XML_ATTR_CONNECTION_PORT);

                                TCPSimulatorProxy newproxy = new TCPSimulatorProxy(host, int.Parse(port));
                                newproxy.messagePosted += new EventHandler<StringEventArgs>(connectionHolder.TCPProxy_MessagePosted);
                                newproxy.taskRequestReceived += new EventHandler(connectionHolder.proxyTaskRequestReceived);
                                newproxy.resultsReceived += new statisticsResultReceivedEventHandler(connectionHolder.proxyResultsReceived);
                                connectionHolder.TCPConnections.Add(newproxy);
                                break;

                            default:
                                throw new Exception(string.Format("Unrecognised tag '{}' found in xml representation of the acquisition metadata. The file may be corrupt.", textReader.Name));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ErrorNotifier.showError(e.Message);
            }
            finally
            {
                if (textReader != null)
                {
                    textReader.Close();
                }
            }
        }

        public static SimulationOptions loadSimulationOptionsFromDisk()
        {
            try
            {
                if (File.Exists(SIM_SETTINGS_FILE_NAME))
                {
                    SimulationOptions simulationOptions = null;
                    FileStream fileStream = new FileStream(SIM_SETTINGS_FILE_NAME, FileMode.Open);
                    simulationOptions = (SimulationOptions)formatter.Deserialize(fileStream);
                    fileStream.Close();
                    return simulationOptions;
                }
                return new SimulationOptions();
            }
            catch
            {
                return new SimulationOptions();
            }
        }

        public static void saveSettingsToDisk(ApplicationOptionsClient applicationOptions, IEnumerable<TCPSimulatorProxy> tcpConnections, SimulationOptions simulationOptions)
        {
            // 1. simulation settings (soap)
            FileStream fileStream = new FileStream(SIM_SETTINGS_FILE_NAME, FileMode.OpenOrCreate);
            formatter.Serialize(fileStream, simulationOptions);
            fileStream.Close();

            // 2. app settings (xml)
            XmlTextWriter textWriter = new XmlTextWriter(APP_SETTINGS_FILE_NAME, null);

            // open the document 
            textWriter.WriteStartDocument();

            // open root
            textWriter.WriteWhitespace("\n");
            textWriter.WriteStartElement(XML_TAG_ROOT);

            // language
            textWriter.WriteWhitespace("\n\t");
            textWriter.WriteStartElement(XML_TAG_LANGUAGE);
            textWriter.WriteAttributeString(XML_ATTR_LANGUAGE_ID, Localization.LanguageSelector.Global.SelectedLanguage.identifier);
            textWriter.WriteEndElement();

            // path to benchmarks
            textWriter.WriteWhitespace("\n\t");
            textWriter.WriteStartElement(XML_TAG_TRACE_PATH_MAIN);
            textWriter.WriteAttributeString(XML_ATTR_VALUE, applicationOptions.TracePathMain);
            textWriter.WriteEndElement();

            // the booleans
            textWriter.WriteWhitespace("\n\t");
            textWriter.WriteStartElement(XML_TAG_IsPredictorComparison);
            textWriter.WriteAttributeString(XML_ATTR_VALUE, applicationOptions.IsPredictorCompareMode.ToString());
            textWriter.WriteEndElement();

            textWriter.WriteWhitespace("\n\t");
            textWriter.WriteStartElement(XML_TAG_SHOW_AM);
            textWriter.WriteAttributeString(XML_ATTR_VALUE, applicationOptions.ShowAM.ToString());
            textWriter.WriteEndElement();
            textWriter.WriteWhitespace("\n\t");
            textWriter.WriteStartElement(XML_TAG_SHOW_GM);
            textWriter.WriteAttributeString(XML_ATTR_VALUE, applicationOptions.ShowGM.ToString());
            textWriter.WriteEndElement();
            textWriter.WriteWhitespace("\n\t");
            textWriter.WriteStartElement(XML_TAG_SHOW_HM);
            textWriter.WriteAttributeString(XML_ATTR_VALUE, applicationOptions.ShowHM.ToString());
            textWriter.WriteEndElement();
            textWriter.WriteWhitespace("\n\t");
            textWriter.WriteStartElement(XML_TAG_SHOW_LINE);
            textWriter.WriteAttributeString(XML_ATTR_VALUE, applicationOptions.ShowLine.ToString());
            textWriter.WriteEndElement();

            // connections
            textWriter.WriteWhitespace("\n\t");
            textWriter.WriteStartElement(XML_TAG_CONNECTIONS);
            foreach (var connection in tcpConnections)
            {
                textWriter.WriteWhitespace("\n\t\t");
                textWriter.WriteStartElement(XML_TAG_CONNECTION);
                textWriter.WriteAttributeString(XML_ATTR_CONNECTION_HOSTNAME, connection.Hostname);
                textWriter.WriteAttributeString(XML_ATTR_CONNECTION_PORT, connection.Port.ToString());
                textWriter.WriteEndElement();
            }
            textWriter.WriteWhitespace("\n\t");
            textWriter.WriteEndElement();

            // close root
            textWriter.WriteWhitespace("\n");
            textWriter.WriteEndElement();

            // end the document.              
            textWriter.WriteEndDocument();

            // close writer
            textWriter.Close();
        }
    }
}
