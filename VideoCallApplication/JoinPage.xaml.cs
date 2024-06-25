/*
 * JoinPage.xaml.cs
 * Author: Henry Glenn
 */

using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Navigation;

namespace VideoCallApplication
{
    /// <summary>
    /// Interaction logic for ClientPage.xaml
    /// </summary>
    public partial class ClientPage : NavigationPage
    {
        private Client _client = new Client();

        public ClientPage(NavigationService navigation) : base(navigation)
        {
            InitializeComponent();
            _client.OnConnectResponse += OnConnectionResponse;
        }

        private void OnConnectionResponse(Communication.ConnectionCode code, Client client)
        {
            if (code == Communication.ConnectionCode.Accepted)
            {
                Dispatcher.Invoke(() =>
                {
                    Navigation.Navigate(new ConnectionPage(Navigation, client));
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    MessagePopupBox.Show(this, "Error", "No host was found at the address provided.");
                });
            } 
        }

        private void OnBackClick(object sender, RoutedEventArgs e)
        {
            Navigation.Navigate(new StartPage(Navigation));
        }

        private void OnConnectClick(object sender, RoutedEventArgs e)
        {
            string[] splitAddress = uxAddressAndPort.Text.Split(":");
            try
            {
                _client.Connect(IPAddress.Parse(splitAddress[0]), int.Parse(splitAddress[1]));
            }
            catch (FormatException)
            {

                MessagePopupBox.Show(this, "Error", "An invalid address was entered.");
            }
            catch (IndexOutOfRangeException)
            {
                MessagePopupBox.Show(this, "Error", "An invalid address was entered.");
            }
            catch (SocketException)
            {
                MessagePopupBox.Show(this, "Error", "No host was found at the address provided.");
            }
        }
    }
}