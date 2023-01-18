namespace Revit_glTF_Exporter
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Common_glTF_Exporter.Utils;
    using Common_glTF_Exporter.ViewModel;

    /// <summary>
    /// Progress Bar Window.
    /// </summary>
    public partial class ProgressBarWindow : Window
    {
        public ProgressBarWindow()
        {
            this.InitializeComponent();
            MainView = this;
            this.DataContext = this;
        }

        public static ProgressBarWindow MainView { get; set; }

        public static ProgressBarWindowViewModel ViewModel { get; set; } = new ProgressBarWindowViewModel();

        public static ProgressBarWindow Create(double maxValue, double currentValue, string message)
        {
            var progressBar = new ProgressBarWindow();
            ProgressBarWindow.ViewModel.ProgressBarGraphicValue = maxValue * 0.07;
            ProgressBarWindow.ViewModel.ProgressBarValue = currentValue;
            ProgressBarWindow.ViewModel.Message = message;
            ProgressBarWindow.ViewModel.ProgressBarMax = maxValue;
            ProgressBarWindow.ViewModel.ProgressBarPercentage = 0;
            progressBar.Show();
            ProgressBarWindow.MainView.Topmost = true;
            return progressBar;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            if (ProgressBarWindow.MainView != null && ProgressBarWindow.MainView.IsActive)
            {
                ProgressBarWindow.MainView.Close();
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CancelProcess_Click(object sender, RoutedEventArgs e)
        {
            GLTFExportContext.cancelation = true;
            this.Close();
        }
    }
}
