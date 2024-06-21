using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Navigation;

namespace VideoCallApplication
{
    /// <summary>
    /// Interaction logic for ClientPage.xaml
    /// </summary>
    public partial class ClientPage : NavigationPage
    {
        public ClientPage(NavigationService navigation) : base(navigation)
        {
            InitializeComponent();
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            string[] splitAddress = uxAddressAndPort.Text.Split(":");
            Debug.Print($"{splitAddress[0]}:{splitAddress[1]}");
            Client client = new Client();
            client.Connect(IPAddress.Parse(splitAddress[0]), int.Parse(splitAddress[1]));

            Navigation.Navigate(new ConnectionPage(Navigation, client));
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //_webcam.Start();

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