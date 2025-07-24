namespace Revit_glTF_Exporter
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Common_glTF_Exporter;
    using Common_glTF_Exporter.Utils;
    using Common_glTF_Exporter.ViewModel;

    /// <summary>
    /// Feedback Window.
    /// </summary>
    public partial class AboutUsWindow : Window
    {
        public AboutUsWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;
            Theme.ApplyDarkLightMode(this.Resources.MergedDictionaries[0]);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void CancelProcess_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Title_Link(object sender, RoutedEventArgs e)
        {
            Hyperlink.Run(Links.contactLink);
        }
        private void everse_Link(object sender, RoutedEventArgs e)
        {
            Hyperlink.Run(Links.everseWebsite);
        }
    }
}
