using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BranchPredictionSimulator
{
    /// <summary>
    /// Interaction logic for ConnectionsAddWindow.xaml
    /// </summary>
    public partial class ConnectionsAddWindow : Window
    {
        public ConnectionsWindow caller = null;

        public ConnectionsAddWindow()
        {
            InitializeComponent();
        }

        private void btn_Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TCPSimulatorProxy tcpSimulatorProxy = new TCPSimulatorProxy(tb_host.Text, int.Parse(tb_port.Text));
                tcpSimulatorProxy.messagePosted += new EventHandler<StringEventArgs>(caller.TCPProxy_MessagePosted);
                tcpSimulatorProxy.taskRequestReceived += new EventHandler(caller.ParentWindow.proxyTaskRequestReceived);
                tcpSimulatorProxy.resultsReceived += new statisticsResultReceivedEventHandler(caller.ParentWindow.proxyResultsReceived);
                caller.ConnectionProxys.Add(tcpSimulatorProxy);
            }
            catch (Exception)
            {
            }
            finally
            {
                this.Hide();
            }
        }
    }
}
