using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
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
    public partial class ClientPage : Page
    {
        public NavigationService Navigation { get; }
        private TcpClient _client = new TcpClient();
        public ClientPage(NavigationService navigation)
        {
            Navigation = navigation;
            InitializeComponent();
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            string[] splitAddress = uxAddressAndPort.Text.Split(":");
            Debug.Print($"{splitAddress[0]}:{splitAddress[1]}");
            try
            {
                _client = new TcpClient();
                _client.Connect(IPAddress.Parse(splitAddress[0]), int.Parse(splitAddress[1]));
                using (NetworkStream stream = _client.GetStream())
                {
                    for (long i = 0; i < 2000; i++)
                    {
                        Communication.WriteToNetworkStream(stream, new MemoryStream(BitConverter.GetBytes(i)));
                    }
                }
                _client.Close();
            }
            catch (Exception exc)
            {
                Debug.Print(exc.ToString());
            }
        }
    }
}
