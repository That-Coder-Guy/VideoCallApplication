using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace VideoCallApplication
{
    /// <summary>
    /// Interaction logic for ClientPage.xaml
    /// </summary>
    public partial class ClientPage : ClosablePage
    {
        public NavigationService Navigation { get; }
        private Client _client = new Client();
        private Webcam _webcam = new Webcam();

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

        private void OnNewFrame(Bitmap bitmap)
        {
            if (_client.IsConnected)
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Bmp);
                    memory.Position = 0;
                    _client.Send(memory);
                }
            }
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _webcam.OnNewFrame += OnNewFrame;
            _webcam.Start();

            /*
            uxFrame.Dispatcher.Invoke(() =>
            {
                Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;
                Thread currentThread = currentDispatcher.Thread;
                Debug.Print($"Invoked Thread ID: {currentThread.ManagedThreadId}");
                uxFrame.Source = _webcam.GetFrame();
            });
            */
        }
    }
}