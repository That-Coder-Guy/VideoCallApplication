using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace VideoCallApplication
{
    public partial class NavigationPage : Page
    {
        public event CancelEventHandler? Closing;
        public NavigationService Navigation { get; }

        public NavigationPage(NavigationService navigation)
        {
            Navigation = navigation; 
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window == null)
            {
                throw new InvalidOperationException("Window is inaccessible");
            }
            else
            {
                window.Closing += Closing;
            }
        }
    }
}
