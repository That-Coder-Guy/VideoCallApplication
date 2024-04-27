using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VideoCallApplication
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : Page
    {
        public NavigationService Navigation { get; }
        public StartPage(NavigationService navigation)
        {
            Navigation = navigation;
            InitializeComponent();
        }

        private void OnJoinClick(object sender, RoutedEventArgs e)
        {
            Navigation.Navigate(new ClientPage(Navigation));
        }

        private void OnHostButtonClick(object sender, RoutedEventArgs e)
        {
            Navigation.Navigate(new HostPage(Navigation));
        }
    }
}
