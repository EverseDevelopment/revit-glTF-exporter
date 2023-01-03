namespace Revit_glTF_Exporter
{
    using System.Windows;
    using System.Windows.Input;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
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

        public ProgressBarWindowViewModel ViewModel { get; set; } = new ProgressBarWindowViewModel();

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
    }
}
