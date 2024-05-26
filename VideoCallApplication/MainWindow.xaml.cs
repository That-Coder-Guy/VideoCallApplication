using System.Windows;

namespace VideoCallApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            uxNavigationFrame.Content = new StartPage(uxNavigationFrame.NavigationService);
        }

        private void OnClosed(object sender, EventArgs e)
        {

        }
    }
}