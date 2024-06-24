using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Effects;

namespace VideoCallApplication
{
    /// <summary>
    /// Interaction logic for InformationPopupBox.xaml
    /// </summary>
    public partial class MessagePopupBox : Window
    {
        public MessagePopupBox(UIElement parent)
        {
            InitializeComponent();
            Owner = GetWindow(parent);
        }
        
        public static bool? Show(UIElement parent, string title, string message)
        {
            MessagePopupBox popup = new MessagePopupBox(parent);
            return popup.ShowDialog(title, message);
        }

        public bool? ShowDialog(string title, string message)
        {
            Title = title;
            uxTitle.Content = title;
            uxMessage.Text = message;
            BlurEffect blur = new BlurEffect();
            blur.Radius = 3;

            Owner.Effect = blur;
            bool? result = ShowDialog();
            Owner.Effect = null;
            return result;
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
