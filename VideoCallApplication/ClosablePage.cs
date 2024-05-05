using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace VideoCallApplication
{
    public partial class ClosablePage : Page
    {
        public event CancelEventHandler? Closing;

        public ClosablePage()
        {
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
