namespace Revit_glTF_Exporter
{
    using System.Windows;
    using System.Windows.Input;
    using Common_glTF_Exporter;
    using Common_glTF_Exporter.Utils;

    /// <summary>
    /// Interaction logic for Settings.xaml.
    /// </summary>
    public partial class MessageWindow : Window
    {
        public MessageWindow()
        {
            this.InitializeComponent();
            Theme.ApplyDarkLightMode(this.Resources.MergedDictionaries[0]);
        }

        public static void Show(string title, string message)
        {
            MessageWindow dlg = new MessageWindow();
            dlg.Title = title;
            dlg.labelMessage.Text = message;
            dlg.ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Title_Link(object sender, RoutedEventArgs e)
        {
            Hyperlink.Run(Links.everseWebsite);
        }
    }
}
