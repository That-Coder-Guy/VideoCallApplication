using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Controls;
using AForge.Video.DirectShow;
using static System.Net.WebRequestMethods;
using System;
using System.Threading.Channels;

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
            Dispatcher.Invoke(() =>
            {
                if (uxPreviewExpander.IsExpanded)
                {
                    uxPreview.Source = Webcam.ToBitmapImage(bitmap);
                }
                else if (uxPreview.Source != null)
                {
                    uxPreview.Source = null;
                }
            });

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

        private void OnSelectedInputEjected(VideoCaptureDevice? device)
        {
            Dispatcher.Invoke(() =>
            {
                uxInputOptions.SelectedItem = null;
            });
            
        }

        private void OnDisconnect(Client client)
        {
            Debug.Print($"{client.RemoteEndPoint} : Disconnected");
            _webcam.Deselect();
            MessageBox.Show("Call has been ended by peer.", "Notice", MessageBoxButton.OK, MessageBoxImage.Warning);
            Dispatcher.Invoke(() =>
            {
                Navigation.Navigate(new StartPage(Navigation));
            });
        }

        private void OnMessageReceived(MemoryStream stream, Client client)
        {
            Dispatcher.Invoke(() =>
            {
                uxFeed.Source = Webcam.ToBitmapImage(new Bitmap(stream));
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
                    new Thread(() =>
                    {
                        _webcam.Deselect();
                        _client.Disconnect();
                    }).Start();
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
            /*
             * NOTE:
             * The following code is inside a temp thread because for some reason running this code
             * syncronously causes the app to freeze thus causing an error to occur. The code that freezes
             * the app is the "_client.Disconnect();" line.
             */
            new Thread(() =>
            {
                _webcam.Deselect();
                _client.Disconnect();
                Dispatcher.Invoke(() =>
                {
                    Navigation.Navigate(new StartPage(Navigation));
                });
            }).Start();
        }
    }
}