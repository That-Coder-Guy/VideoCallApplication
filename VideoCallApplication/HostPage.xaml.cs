﻿using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
        }

        private void OnClientConnect(Client client)
        {
            Dispatcher.Invoke(() =>
            {
                Navigation.Navigate(new ConnectionPage(Navigation, client));
            });
            _videoServer.Deafen();
            _videoServer.Close();
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (_videoServer.IsListening)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to stop listening for connections?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
                else
                {
                    _videoServer.Deafen();
                    _videoServer.Close();
                }
            }
        }
    }
}
