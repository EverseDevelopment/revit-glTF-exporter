namespace Revit_glTF_Exporter
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Common_glTF_Exporter.Utils;
    using Common_glTF_Exporter.ViewModel;
    using Common_glTF_Exporter;

    /// <summary>
    /// Feedback Window.
    /// </summary>
    public partial class FeedbackWindow : Window
    {
        public FeedbackWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;
            Theme.ApplyDarkLightMode(this.Resources.MergedDictionaries[0]);
        }


        public static FeedbackWindow Create()
        {
            var progressBar = new FeedbackWindow();
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
            Hyperlink.Run(Links.contactLink);
        }
        private void Leia_Link(object sender, RoutedEventArgs e)
        {
            Hyperlink.Run("https://e-verse.com/leia/");
        }
    }
}
