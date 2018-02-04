using System;
using System.Collections.Generic;
using System.Linq;

using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using System.IO;

using PredictionLogic;

namespace BranchPredictionSimulatorServer
{
    class Program
    {
        public static ApplicationOptionsServer applicationOptions;
        
        static void Main(string[] args)
        {
            // wire up error notifier action
            ErrorNotifier.errorNotifierAction = new ErrorNotifierActionServer();
            
            applicationOptions = new ApplicationOptionsServer();
            if (!applicationOptions.loadOptions())
            {
                Console.WriteLine("The application will now exit.");
                Console.WriteLine("Press any key...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Starting server.");
            int portsCount = applicationOptions.listeningPorts.Count;
            Console.Write("Listening on ports: " + applicationOptions.listeningPorts[0]);
            for (int i = 1; i < portsCount; i++)
            {
                Console.Write(", " + applicationOptions.listeningPorts[i]);
            }
            Console.WriteLine();

            TcpListener[] tcpListners = new TcpListener[portsCount];
            for (int i = 0; i < portsCount; i++)
            {
                tcpListners[i] = new TcpListener(IPAddress.Any, applicationOptions.listeningPorts[i]);
                tcpListners[i].Start(); 
            }

            Console.WriteLine();
            Console.WriteLine("Waiting for clients...");
            while (true)
            {
                int clientPending = -1;
                for (int i = 0; i < portsCount; i++)
                {
                    if (tcpListners[i].Pending())
                    {
                        clientPending = i;
                        break;
                    }
                }
                if (clientPending == -1)
                {
                    Thread.Sleep(5000);
                }
                else
                {
                    ConnectionThread newConnection = new ConnectionThread(tcpListners[clientPending], applicationOptions.listeningPorts[clientPending]);
                }
            }
        }       
    }
}
