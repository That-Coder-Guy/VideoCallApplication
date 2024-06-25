/*
 * HostPage.xaml.cs
 * Author: Henry Glenn
 */

using System.ComponentModel;
using System.Windows;
using System.Windows.Navigation;

namespace VideoCallApplication
{
    /// <summary>
    /// Interaction logic for HostPage.xaml
    /// </summary>
    public partial class HostPage : NavigationPage
    {
        private Server _videoServer = new Server(1);

        public HostPage(NavigationService navigation) : base(navigation)
        {
            _videoServer.OnClientConnect += OnClientConnect;
            InitializeComponent();
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            _videoServer.Listen();
            uxAddressAndPort.Text = $"{Communication.GetIPv4Addresses()[0]}:{_videoServer.Port}";
            uxStart.IsEnabled = false;
        }

        private void OnClientConnect(Client client)
        {
            Dispatcher.Invoke(() =>
            {
                Navigation.Navigate(new ConnectionPage(Navigation, client));
            });
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (_videoServer.IsListening)
            {
                bool? result = QuestionPopupBox.Show(this, "Confirm", "Are you sure you want to stop listening for connections?");
                if (result == true)
                {
                    _videoServer.Deafen();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_videoServer.IsListening)
            {
                _videoServer.Deafen();
            }
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            if (_videoServer.IsListening)
            {
                bool? result = QuestionPopupBox.Show(this, "Confirm", "Are you sure you want to stop listening for connections?");
                if (result == true)
                {
                    _videoServer.Deafen();
                    Navigation.Navigate(new StartPage(Navigation));
                }
            }
            else
            {
                Navigation.Navigate(new StartPage(Navigation));
            }
        }
    }
}
