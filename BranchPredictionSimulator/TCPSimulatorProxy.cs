using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using PredictionLogic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;
using PredictionLogic.Prediction;
using PredictionLogic.CommunicationProtocol;
using PredictionLogic.SimulationStatistics;
using PredictionLogic.Simulation;

namespace BranchPredictionSimulator
{
    public class TCPSimulatorProxy : INotifyPropertyChanged
    {
        public event statisticsResultReceivedEventHandler resultsReceived;
        public event EventHandler taskRequestReceived;
        public event EventHandler<StringEventArgs> messagePosted;

        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private readonly IFormatter formatter;
        private Dictionary<uint, SimulationInfo> sentSimulations = new Dictionary<uint, SimulationInfo>();
        private static uint nextTransmissionID = 1;

        private Thread listeningThread = null;
        private bool isConnectionThreadRunning = false;

        private string hostname;
        public string Hostname
        {
            get
            {
                return hostname;
            }
        }

        private int port;
        public int Port
        {
            get
            {
                return port;
            }
        }

        private bool connected;
        public bool Connected
        {
            get
            {
                return connected;
            }
        }

        private int taskRequests = 0;
        public int TaskRequests
        {
            get
            {
                return taskRequests;
            }
        }

        private SimulationSession session = null;
        public SimulationSession Session
        {
            get
            {
                return session;
            }
        }

        public TCPSimulatorProxy(string hostname, int port)
        {
            formatter = new BinaryFormatter();
            formatter.Binder = new ClientToServerDeserializationBinder();
            this.hostname = hostname;
            this.port = port;
            this.connected = false;
            taskRequests = 0;
        }

        public void tryConnect()
        {
            try
            {
                tcpClient = new TcpClient(hostname, port);
                networkStream = tcpClient.GetStream();
                connected = true;
                networkStream.WriteByte((byte)TrasmissionFlags.ClientName);
                formatter.Serialize(networkStream, Environment.MachineName);
                networkStream.Flush();

                postMessage("Connected to " + hostname + " on port " + port);

                isConnectionThreadRunning = true;
                listeningThread = new Thread(handleConnection);
                listeningThread.Start();
            }
            catch (Exception e)
            {
                connected = false;
                postMessage(e.Message);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, isConnectedPropertyChangedEventArgs);
            }
        }

        public void disconnect()
        {
            try
            {
                networkStream.Close();
                tcpClient.Close();
            }
            catch
            {
            }
            connected = false;
            if (PropertyChanged != null)
            {
                PropertyChanged(this, isConnectedPropertyChangedEventArgs);
            }
            if (listeningThread != null)
            {
                listeningThread.Abort();
            }
        }

        public void startNewSession(SimulationSession session)
        {
            if (session == null)
            {
                throw new ArgumentNullException("session");
            }

            this.taskRequests = 0;
            this.session = session;

            try
            {
                networkStream.WriteByte((byte)TrasmissionFlags.NewSession);
                formatter.Serialize(networkStream, session);
                postMessage("New session request sent (session id: " + session.sessionID + ")");
            }
            catch (Exception e)
            {
                postMessage(e.Message);
            }
        }

        public void sendSimulationTask(SimulationInfo simulation)
        {
            if (this.session == null)
            {
                postMessage("Cannot send task; no session is active at the moment. ");
                return;
            }

            taskRequests--;
            TaskPackage taskPackage = new TaskPackage
            {
                taskID = nextTransmissionID++,
                sessionID = this.session.sessionID,
                simulationTask = simulation
            };

            try
            {
                networkStream.WriteByte((byte)TrasmissionFlags.Task);
                formatter.Serialize(networkStream, taskPackage);
                postMessage("Task sent to " + hostname + " on port " + port + " (session id: " + this.session.sessionID + ")");
                sentSimulations.Add(taskPackage.taskID, simulation);
            }
            catch (Exception e)
            {
                postMessage(e.Message);
            }
        }

        public void sendAbortSessionRequest()
        {
            try
            {
                networkStream.WriteByte((byte)TrasmissionFlags.AbortSession);
                formatter.Serialize(networkStream, this.session.sessionID);
                postMessage("Session abortion request sent to " + hostname + " on port " + port + " (session id: " + this.session.sessionID + ")");
                taskRequests = 0;
            }
            catch (Exception e)
            {
                postMessage(e.Message);
            }
        }

        private void handleConnection()
        {
            int header;
            uint sessionIdReceived;
            try
            {
                while (isConnectionThreadRunning)
                {
                    header = networkStream.ReadByte();
                    if (header == -1)
                    {
                        break;
                    }
                    switch (header)
                    {
                        case (byte)TrasmissionFlags.TaskRequest:
                            sessionIdReceived = (uint)formatter.Deserialize(networkStream);
                            if (sessionIdReceived == this.session.sessionID)
                            {
                                if (taskRequestReceived != null)
                                {
                                    taskRequestReceived(this, EventArgs.Empty);
                                }
                                taskRequests++;
                                postMessage("Received a task request from " + hostname + " on port " + port + " ");
                            }
                            else
                            {
                                postMessage("Received a task request from " + hostname + " on port " + port + ", but with the wrong session ID ");
                            }
                            break;
                        case (byte)TrasmissionFlags.Result:
                            ResultPackage res_pack = (ResultPackage)formatter.Deserialize(networkStream);
                            if (res_pack.sessionID == this.session.sessionID)
                            {
                                if (resultsReceived != null)
                                {
                                    resultsReceived(this, new BenchmarkStatisticsResultReceivedEventArgs(sentSimulations[res_pack.taskID], res_pack.result));
                                }
                                postMessage("Received a result from " + hostname + " on port " + port + " ");
                            }
                            else
                            {
                                postMessage("Received a result from " + hostname + " on port " + port + ", but with the wrong session ID ");
                            }
                            break;
                        default:
                            postMessage("Invalid message header received: " + header);
                            isConnectionThreadRunning = false;
                            break;
                    }

                }
            }
            catch (Exception e)
            {
                postMessage(e.Message);
            }
            finally
            {
                postMessage("Connection to " + hostname + " on port " + port + " terminated");
                connected = false;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, isConnectedPropertyChangedEventArgs);
                }
            }
        }

        private void postMessage(string message)
        {
            if (messagePosted != null)
            {
                messagePosted(this, new StringEventArgs(message));
            }
        }

        #region INotifyPropertyChanged

        private PropertyChangedEventArgs isConnectedPropertyChangedEventArgs = new PropertyChangedEventArgs("Connected");

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    public delegate void statisticsResultReceivedEventHandler(object sender, BenchmarkStatisticsResultReceivedEventArgs e);

    public class BenchmarkStatisticsResultReceivedEventArgs : EventArgs
    {
        public readonly SimulationInfo simulation;
        public readonly BenchmarkStatisticsResult result;

        public BenchmarkStatisticsResultReceivedEventArgs(SimulationInfo simulation, BenchmarkStatisticsResult result)
        {
            this.simulation = simulation;
            this.result = result;
        }
    }

    public class StringEventArgs : EventArgs
    {
        public readonly string message;

        public StringEventArgs(string message)
        {
            this.message = message;
        }
    }
}
