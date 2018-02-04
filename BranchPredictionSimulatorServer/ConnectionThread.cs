using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Runtime.Serialization;

using PredictionLogic;
using PredictionLogic.Prediction;
using PredictionLogic.SimulationStatistics;
using PredictionLogic.CommunicationProtocol;
using PredictionLogic.Simulation;

namespace BranchPredictionSimulatorServer
{
    public class ConnectionThread
    {
        private TcpListener tcpListener;
        private int port;
        private BinaryFormatter formatter;

        private TcpClient tcpClient;
        private NetworkStream networkStream;

        private object networkStreamLocker = new object();

        private SimulationSession simulationSession = null;
        public SimulationSession SimulationSession
        {
            get
            {
                return simulationSession;
            }
        }

        private bool requestClose;
        private int header;
        private string clientName;

        private Thread connectionThread;

        private int numberOfWorkerThreads;
        private SimulationWorkerThread[] workerThreads;

        private Queue<TaskPackage> taskBuffer;
        public Queue<TaskPackage> TaskBuffer
        {
            get
            {
                return taskBuffer;
            }
        }

        public ConnectionThread(TcpListener tcpListener, int port)
        {
            requestClose = false;
            formatter = new BinaryFormatter();
            formatter.Binder = new ClientToServerDeserializationBinder();

            this.tcpListener = tcpListener;
            this.port = port;

            taskBuffer = new Queue<TaskPackage>();

            numberOfWorkerThreads = Environment.ProcessorCount;
            workerThreads = new SimulationWorkerThread[numberOfWorkerThreads];
            for (int i = 0; i < numberOfWorkerThreads; i++)
            {
                workerThreads[i] = new SimulationWorkerThread(this, "worker thread # " + i);
            }

            connectionThread = new Thread(handleConnection);
            connectionThread.Name = "Connection thread";
            connectionThread.Start();
        }

        public void requestClosing()
        {
            requestClose = true;
        }

        public void handleConnection()
        {
            tcpClient = tcpListener.AcceptTcpClient();
            networkStream = tcpClient.GetStream();

            // initialization
            header = networkStream.ReadByte();
            if (header != (byte)TrasmissionFlags.ClientName)
            {
                Console.WriteLine("Client does not respect protocol.");
                networkStream.Close();
                tcpClient.Close();
                return;
            }

            try
            {
                clientName = (string)formatter.Deserialize(networkStream);
                Console.WriteLine("Connected to " + clientName + " on port " + port);
            }
            catch
            {
                Console.WriteLine("Client does not respect protocol.");
                networkStream.Close();
                tcpClient.Close();
                return;
            }

            // main message loop
            try
            {
                while (!requestClose)
                {
                    header = networkStream.ReadByte();
                    if (header == -1)
                    {
                        break;
                    }
                    switch (header)
                    {
                        case (byte)TrasmissionFlags.NewSession:
                            this.simulationSession = (SimulationSession)formatter.Deserialize(networkStream);
                            Console.WriteLine("Starting new session with " + clientName + " (session id: " + this.simulationSession.sessionID + ")");
                            for (int i = 0; i < numberOfWorkerThreads; i++)
                            {
                                workerThreads[i].abortCurrentTask();
                                sendTaskRequest();
                            }
                            lock (taskBuffer) // cleanse taskBuffer of older parasite tasks
                            {
                                int taskBufferCount = taskBuffer.Count;
                                for (int i = 0; i < taskBufferCount; i++)
                                {
                                    TaskPackage t = taskBuffer.Dequeue();
                                    if (t.sessionID == this.simulationSession.sessionID)
                                    {
                                        taskBuffer.Enqueue(t);
                                    }
                                }
                            }
                            break;

                        case (byte)TrasmissionFlags.Task:
                            TaskPackage taskPackage = (TaskPackage)formatter.Deserialize(networkStream);
                            if (this.simulationSession != null && taskPackage.sessionID == this.simulationSession.sessionID)
                            {
                                lock (taskBuffer)
                                {
                                    taskBuffer.Enqueue(taskPackage);
                                }
                                for (int i = 0; i < numberOfWorkerThreads; i++)
                                {
                                    workerThreads[i].awake();
                                }
                                Console.WriteLine("TaskPackage received from " + clientName);
                            }
                            else
                            {
                                Console.WriteLine("TaskPackage received from " + clientName + ", but belonging to a closed session.");
                            }
                            break;

                        case (byte)TrasmissionFlags.AbortSession:
                            uint sessionIdAborted = (uint)formatter.Deserialize(networkStream);
                            if (this.simulationSession != null && sessionIdAborted == this.simulationSession.sessionID)
                            {
                                for (int i = 0; i < numberOfWorkerThreads; i++)
                                {
                                    if (workerThreads[i] != null) workerThreads[i].abortCurrentTask();
                                }
                                Console.WriteLine("Session " + sessionIdAborted + " aborted.");
                            }
                            else
                            {
                                Console.WriteLine("Session abort message received (" + sessionIdAborted + ") but not for the current session. ");
                            }
                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("Connection to " + clientName + " terminated");

            networkStream.Close();
            tcpClient.Close();
        }

        public void sendResult(uint taskId, BenchmarkStatisticsResult result)
        {
            lock (networkStreamLocker)
            {
                if (this.simulationSession == null)
                {
                    Console.WriteLine("Error: Cannot send result. No session is active at the moment. ");
                    return;
                }

                ResultPackage resultPackage = new ResultPackage
                {
                    taskID = taskId,
                    sessionID = this.simulationSession.sessionID,
                    result = result
                };

                try
                {
                    networkStream.WriteByte((byte)TrasmissionFlags.Result);
                    formatter.Serialize(networkStream, resultPackage);
                    Console.WriteLine("Result sent back to " + clientName);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Attempt to send result back to " + clientName + " failed: " + e.Message);
                }
            }
        }

        public void sendTaskRequest()
        {
            lock (networkStreamLocker)
            {
                if (this.simulationSession == null)
                {
                    Console.WriteLine("Error: Cannot send task request. No session is active at the moment. ");
                    return;
                }

                try
                {
                    networkStream.WriteByte((byte)TrasmissionFlags.TaskRequest);
                    formatter.Serialize(networkStream, this.simulationSession.sessionID);
                    Console.WriteLine("Task request sent to " + clientName + ".");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Attempt to send a task request to " + clientName + " failed: " + e.Message);
                }
            }
        }
    }
}
