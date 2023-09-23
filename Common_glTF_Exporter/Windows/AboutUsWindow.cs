namespace Revit_glTF_Exporter
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
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
        }


        public static AboutUsWindow Create()
        {
            var progressBar = new AboutUsWindow();
            progressBar.Show();
            return progressBar;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (ProgressBarWindow.MainView != null && ProgressBarWindow.MainView.IsActive)
            {
                ProgressBarWindow.MainView.Close();
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CancelProcess_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Title_Link(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://e-verse.com/contact/");
        }
        private void Leia_Link(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://e-verse.com/leia/");
        }
    }
}
