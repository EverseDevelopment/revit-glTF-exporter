using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Common_glTF_Exporter.ViewModel;
using Common_glTF_Exporter.Utils;
using System.Threading;
using Common_glTF_Exporter.Windows.MainWindow;

namespace Revit_glTF_Exporter
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        View _view;
        private UnitsViewModel _unitsViewModel;

        public static MainWindow MainView { get; set; }

        public MainWindow(Document doc, View view)
        {
            _unitsViewModel = new UnitsViewModel();
            this.DataContext = _unitsViewModel;
            MainView = this;

            InitializeComponent();

            ComboUnits.Set(doc, UnitTextBlock);
            this._view = view;

            UpdateForm.Run(MainWindow_Border);
        }

        private void OnExportView(object sender, RoutedEventArgs e)
        {
            if (_view.GetType().Name != "View3D")
            {
                this.Hide();
                TaskDialog.Show("glTFRevitExport", "You must be in a 3D view to export.");
                this.Close();
                return;
            }
            this.Show();
            View3D exportView = _view as View3D;

            string fileName = SettingsConfig.GetValue("fileName");                                                                                         
            bool dialogResult = FilesHelper.AskToSave(ref fileName, string.Empty, ".gltf");

            if (dialogResult == true)
            {
                string filename = fileName;
                string directory = filename.Replace(".gltf", "");
                string nameOnly = System.IO.Path.GetFileNameWithoutExtension(filename);

                SettingsConfig.Set("path", directory);
                SettingsConfig.Set("fileName", nameOnly);

                ExportView3D(exportView, false);
            }
        }

        public void ExportView3D(View3D view3d, bool mode)
        {
            Document doc = view3d.Document;

            ProgressBarWindow progressBar = new ProgressBarWindow();
            progressBar.ViewModel.ProgressBarValue = 0;
            progressBar.ViewModel.Message = "Converting elements...";
            progressBar.ViewModel.ProgressBarMax = Collectors.AllVisibleElementsByView(doc, doc.ActiveView).Count;
            progressBar.Show();
            ProgressBarWindow.MainView.Topmost = true;

            // Use our custom implementation of IExportContext as the exporter context.
            glTFExportContext ctx = new glTFExportContext(doc, progressBar);

            // Create a new custom exporter with the context.
            CustomExporter exporter = new CustomExporter(doc, ctx);

            exporter.ShouldStopOnError = false;

            exporter.Export(view3d);

            progressBar.ViewModel.Message = "GLTF exportation completed!";
            Thread.Sleep(1000);
            progressBar.Close();
        }
        private void Advanced_Settings_Button(object sender, RoutedEventArgs e)
        {
            _ = this.Advanced_Settings_Grid.Visibility == System.Windows.Visibility.Visible ?
                (this.Advanced_Settings_Grid.Visibility = System.Windows.Visibility.Collapsed) : (this.Advanced_Settings_Grid.Visibility = System.Windows.Visibility.Visible);

            var template = this.AdvancedSettingsButton.Template;

            var slideUpImage = (System.Windows.Shapes.Path)template.FindName("SlideUp_Image", this.AdvancedSettingsButton);
            var slideDownImage = (System.Windows.Shapes.Path)template.FindName("SlideDown_Image", this.AdvancedSettingsButton);

            if (slideUpImage.Visibility == System.Windows.Visibility.Visible)
            {
                slideUpImage.Visibility = System.Windows.Visibility.Hidden;
                slideDownImage.Visibility = System.Windows.Visibility.Visible;
            }
            else if (slideDownImage.Visibility == System.Windows.Visibility.Visible)
            {
                slideUpImage.Visibility = System.Windows.Visibility.Visible;
                slideDownImage.Visibility = System.Windows.Visibility.Hidden;
            }

            _ = this.MainWindow_Window.Height == 700 ? (this.MainWindow_Window.Height = 410) : (this.MainWindow_Window.Height = 700);
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Title_Link(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://e-verse.com/");
        }
        private void TrueFalseToggles(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Primitives.ToggleButton button = sender as System.Windows.Controls.Primitives.ToggleButton;
            SettingsConfig.Set(button.Name, button.IsChecked.ToString());
        }
        private void RadioButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton button = sender as System.Windows.Controls.RadioButton;
            string value = button.Name.Replace("compression", "");
            string key = button.Name.Replace(value, "");
            SettingsConfig.Set(key, value);
        }
    }
}
