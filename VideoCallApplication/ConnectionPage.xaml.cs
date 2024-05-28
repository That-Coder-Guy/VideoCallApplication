using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Navigation;

namespace VideoCallApplication
{
    /// <summary>
    /// Interaction logic for ConnectionPage.xaml
    /// </summary>
    public partial class ConnectionPage : NavigationPage
    {
        private Client _client;

        public ConnectionPage(NavigationService navigation, Client client) : base(navigation)
        {
            InitializeComponent();
            _client = client;
            _client.OnDisconnect += OnDisconnect;
            _client.OnMessageReceived += OnMessageReceived;
            Closing += OnClosing;
        }

        private void OnDisconnect(Client client)
        {
            
        }

        private void OnMessageReceived(MemoryStream stream, Client client)
        {
            
        }

        private void OnClosing(object? sender, CancelEventArgs e)
        {
            if (_client.IsConnected)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to end the call?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
