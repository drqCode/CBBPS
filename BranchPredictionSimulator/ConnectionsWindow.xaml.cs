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
using System.Threading;
using System.IO;

namespace BranchPredictionSimulator
{
    /// <summary>
    /// Interaction logic for ConnectionsWindow.xaml
    /// </summary>
    public partial class ConnectionsWindow : Window
    {
        public ObservableCollection<TCPSimulatorProxy> ConnectionProxys { get; private set; }
        public ObservableCollection<string> ConnectionMessages { get; private set; }
        public WindowMain ParentWindow { get; private set; }

        public ConnectionsAddWindow ConnectionsAddWindow = null;

        public ConnectionsWindow()
        {
            InitializeComponent();
        }

        public void InitWindow(WindowMain parent)
        {
            this.ParentWindow = parent;
            this.ConnectionProxys = parent.TCPConnections;
            this.ConnectionMessages = parent.ConnectionMessages;
            lbConnections.DataContext = ConnectionProxys;
            lbMessages.DataContext = ConnectionMessages;
        }

        private void btn_connect_Click(object sender, RoutedEventArgs e)
        {
            if (this.lbConnections.SelectedIndex == -1)
            {
                return;
            }
            (lbConnections.SelectedItem as TCPSimulatorProxy).tryConnect();
        }

        private void btn_connect_all_Click(object sender, RoutedEventArgs e)
        {
            foreach (var c in ConnectionProxys)
            {
                c.tryConnect();
            }
        }

        private void btn_disconnect_Click(object sender, RoutedEventArgs e)
        {
            if (this.lbConnections.SelectedIndex == -1)
            {
                return;
            }
            (lbConnections.SelectedItem as TCPSimulatorProxy).disconnect();
        }

        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            ConnectionsAddWindow = new ConnectionsAddWindow();
            ConnectionsAddWindow.caller = this;
            ConnectionsAddWindow.Show();
        }

        private void btn_delete_Click(object sender, RoutedEventArgs e)
        {
            if (this.lbConnections.SelectedIndex == -1)
            {
                return;
            }
            ConnectionProxys.RemoveAt(lbConnections.SelectedIndex);
        }

        public void TCPProxy_MessagePosted(object sender, StringEventArgs e)
        {
            if (this.Dispatcher.Thread.Equals(Thread.CurrentThread))
            {
                ConnectionMessages.Add(e.message);
            }
            else
            {
                this.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal, new Action(
                        delegate()
                        {
                            ConnectionMessages.Add(e.message);
                        }
                ));
            }
        }
    }
}
