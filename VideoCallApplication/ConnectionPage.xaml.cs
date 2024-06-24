/*
 * ConnectionPage.cs
 * Author: Henry Glenn
 */

using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Controls;
using AForge.Video.DirectShow;

namespace VideoCallApplication
{
    /// <summary>
    /// Interaction logic for ConnectionPage.xaml
    /// </summary>
    public partial class ConnectionPage : NavigationPage
    {
        private Client _client;
        private Webcam _webcam = new Webcam();

        public ConnectionPage(NavigationService navigation, Client client) : base(navigation)
        {
            InitializeComponent();
            _webcam.OnNewFrame += OnNewFrame;
            _webcam.OnSelectedInputEjected += OnSelectedInputEjected;

            _client = client;
            _client.OnDisconnect += OnDisconnect;
            _client.OnMessageReceived += OnMessageReceived;
            Closing += OnClosing;

            AddInputOptions(_webcam.Inputs);
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

            Dispatcher.BeginInvoke(() =>
            {
                if (uxPreviewExpander.IsExpanded)
                {
                    uxPreview.Source = Webcam.ToBitmapImage(bitmap);
                }
                else if (uxPreview.Source != null)
                {
                    uxPreview.Source = null;
                }
                bitmap.Dispose();
            });
        }

        private void OnSelectedInputEjected(VideoCaptureDevice? device)
        {
            Dispatcher.BeginInvoke(() =>
            {
                uxInputOptions.SelectedItem = null;
            });
            
        }

        private void OnDisconnect(Client client)
        {
            Debug.Print($"{client.RemoteEndPoint} : Disconnected");
            _webcam.Deselect();
            Dispatcher.BeginInvoke(() =>
            {
                MessagePopupBox.Show(this, "Notice", "Call has been ended by peer.");
                Navigation.Navigate(new StartPage(Navigation));
            });
        }

        private void OnMessageReceived(MemoryStream stream, Client client)
        {
            Dispatcher.BeginInvoke(() =>
            {
                using (Bitmap bitmap = new Bitmap(stream))
                {
                    uxFeed.Source = Webcam.ToBitmapImage(bitmap);
                }
            });
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
                    _webcam.Deselect();
                    _client.Disconnect();
                }
            }
        }

        private void OnInputSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (uxInputOptions.SelectedItem is ComboBoxItem selectedItem)
            {
                _webcam.Select((FilterInfo)selectedItem.Tag); 
            }
        }

        private void AddInputOptions(List<FilterInfo> filters)
        {
            foreach (FilterInfo filter in filters)
            {
                ComboBoxItem option = new ComboBoxItem();
                option.Content = filter.Name;
                option.Tag = filter;
                uxInputOptions.Items.Add(option);
            }
        }

        private void uxInputOptionsOpened(object sender, EventArgs e)
        {
            List<FilterInfo> filtersToAdd = _webcam.Inputs;
            List<ComboBoxItem> itemsToRemove = new List<ComboBoxItem>();
            foreach (ComboBoxItem item in uxInputOptions.Items)
            {
                FilterInfo filter = (FilterInfo)item.Tag;
                if (!Webcam.TryRemoveFilter(filter, filtersToAdd))
                {
                    itemsToRemove.Add(item);
                }
            }

            foreach (ComboBoxItem item in itemsToRemove)
            {
                uxInputOptions.Items.Remove(item);
            }

            AddInputOptions(filtersToAdd);
        }

        private void OnEndClick(object sender, RoutedEventArgs e)
        {
            _webcam.Deselect();
            _client.Disconnect();
            Dispatcher.BeginInvoke(() =>
            {
                Navigation.Navigate(new StartPage(Navigation));
            });
        }
    }
}