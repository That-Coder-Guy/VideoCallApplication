/*
 * StartPage.xaml.cs
 * Author: Henry Glenn
 */

using System.Windows;
using System.Windows.Navigation;

namespace VideoCallApplication
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : NavigationPage
    {
        public StartPage(NavigationService navigation) : base(navigation)
        {
            InitializeComponent();
        }

        private void OnJoinClick(object sender, RoutedEventArgs e)
        {
            Navigation.Navigate(new ClientPage(Navigation));
        }

        private void OnHostClick(object sender, RoutedEventArgs e)
        {
            Navigation.Navigate(new HostPage(Navigation));
        }

        private void uxTest_Click(object sender, RoutedEventArgs e)
        {
            Navigation.Navigate(new ConnectionPage(Navigation, new Client()));
        }
    }
}
