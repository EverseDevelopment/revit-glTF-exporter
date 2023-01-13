namespace Revit_glTF_Exporter
{
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Common_glTF_Exporter.Utils;
    using Common_glTF_Exporter.ViewModel;
    using Common_glTF_Exporter.Windows.MainWindow;

    /// <summary>
    /// Interaction logic for Settings.xaml.
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(Document doc, View view)
        {
            this.UnitsViewModel = new UnitsViewModel();
            this.DataContext = this.UnitsViewModel;
            MainView = this;

            this.InitializeComponent();

            ComboUnits.Set(doc, this.UnitTextBlock);
            this.View = view;

            UpdateForm.Run(this.MainWindow_Border);

            LabelVersion.Update(this.UnitsViewModel);
        }

        public static MainWindow MainView { get; set; }

        private View View { get; set; }

        private UnitsViewModel UnitsViewModel { get; set; }

        public void ExportView3D(View3D view3d, bool mode)
        {
            Document doc = view3d.Document;

            ProgressBarWindow progressBar =
                ProgressBarWindow.Create(Collectors.AllVisibleElementsByView(doc, doc.ActiveView).Count, 0, "Converting elements...");

            // Use our custom implementation of IExportContext as the exporter context.
            GLTFExportContext ctx = new GLTFExportContext(doc);

            // Create a new custom exporter with the context.
            CustomExporter exporter = new CustomExporter(doc, ctx);

            exporter.ShouldStopOnError = false;

            #if REVIT2019
            exporter.Export(view3d);
            #else
            exporter.Export(view3d as View);
            #endif

            ProgressBarWindow.ViewModel.Message = "GLTF exportation completed!";
            Thread.Sleep(1000);
            progressBar.Close();
        }

        private void OnExportView(object sender, RoutedEventArgs e)
        {
            View3D exportView = this.View as View3D;

            string fileName = SettingsConfig.GetValue("fileName");
            bool dialogResult = FilesHelper.AskToSave(ref fileName, string.Empty, ".gltf");

            if (dialogResult == true)
            {
                string filename = fileName;
                string directory = filename.Replace(".gltf", string.Empty);
                string nameOnly = System.IO.Path.GetFileNameWithoutExtension(filename);

                SettingsConfig.SetValue("path", directory);
                SettingsConfig.SetValue("fileName", nameOnly);

                this.ExportView3D(exportView, false);
            }
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
            SettingsConfig.SetValue(button.Name, button.IsChecked.ToString());
        }

        private void RadioButtonClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton button = sender as System.Windows.Controls.RadioButton;
            string value = button.Name;
            string key = "compression";
            SettingsConfig.SetValue(key, value);
        }

        private void DigitsSliderValueChanged(object sender, RoutedEventArgs e)
        {
            Slider slider = sender as Slider;
            int value = Convert.ToInt32(slider.Value.ToString());
            string key = "digits";
            SettingsConfig.SetValue(key, value.ToString());
        }
    }
}
