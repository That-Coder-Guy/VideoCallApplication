using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Reg;
using Emgu.CV.Structure;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace VideoCallApplication
{
    /// <summary>
    /// Interaction logic for HostPage.xaml
    /// </summary>
    public partial class HostPage : Page
    {
        public NavigationService Navigation { get; }
        private Server _videoServer = new Server(1);

        public HostPage(NavigationService navigation)
        {
            Navigation = navigation;
            _videoServer.OnClientConnect += OnClientConnect;
            InitializeComponent();
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            _videoServer.Listen();
            _videoServer.OnMessageReceived += OnMessageReceived;
            _videoServer.OnClientDisconnect += OnClientDisconnect;
            uxAddressAndPort.Text = $"{Communication.GetIPv4Addresses()[0]}:{_videoServer.Port}";
        }

        private void OnClientConnect(TcpClient client)
        {
            Debug.Print("Client connected");
        }

        private void OnMessageReceived(TcpClient client, MemoryStream stream)
        {
            Debug.Print($"{client.Client.RemoteEndPoint} : {BitConverter.ToInt64(stream.ToArray(), 0)}");
        }

        private void OnClientDisconnect(EndPoint endpoint)
        {
            Debug.Print($"{endpoint} : Disconnected");
        }
    }
}
