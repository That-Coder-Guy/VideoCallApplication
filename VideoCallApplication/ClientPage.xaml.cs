using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VideoCallApplication
{
    /// <summary>
    /// Interaction logic for ClientPage.xaml
    /// </summary>
    public partial class ClientPage : ClosablePage
    {
        public NavigationService Navigation { get; }
        private Client _client = new Client();
        public ClientPage(NavigationService navigation)
        {
            Navigation = navigation;
            InitializeComponent();
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            string[] splitAddress = uxAddressAndPort.Text.Split(":");
            Debug.Print($"{splitAddress[0]}:{splitAddress[1]}");
            _client.Connect(IPAddress.Parse(splitAddress[0]), int.Parse(splitAddress[1]));
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (_client.IsConnected)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to close client connection?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    _client.Disconnect();
                }
            }
        }
    }
}