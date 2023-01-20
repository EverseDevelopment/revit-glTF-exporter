namespace Revit_glTF_Exporter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;
    using System.Windows.Input;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Common_glTF_Exporter.Utils;
    using Common_glTF_Exporter.ViewModel;
    using Common_glTF_Exporter.Windows.MainWindow;
    using View = Autodesk.Revit.DB.View;

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

            ComboUnits.Set(doc);
            this.View = view;

            UpdateForm.Run(this.MainWindow_Border);

            LabelVersion.Update(this.UnitsViewModel);
        }

        public static MainWindow MainView { get; set; }

        private View View { get; set; }

        private UnitsViewModel UnitsViewModel { get; set; }

        private void OnExportView(object sender, RoutedEventArgs e)
        {
            View3D exportView = this.View as View3D;

            string format = string.Concat(".", SettingsConfig.GetValue("format"));
            string fileName = SettingsConfig.GetValue("fileName");
            bool dialogResult = FilesHelper.AskToSave(ref fileName, string.Empty, format);

            if (dialogResult != true)
            {
                return;
            }

            string directory = fileName.Replace(format, string.Empty);
            string nameOnly = System.IO.Path.GetFileNameWithoutExtension(fileName);

            SettingsConfig.SetValue("path", directory);
            SettingsConfig.SetValue("fileName", nameOnly);

            Document doc = exportView.Document;

            List<Element> elementsInView = Collectors.AllVisibleElementsByView(doc, doc.ActiveView);

            if (!elementsInView.Any())
            {
                MessageWindow.Show("No Valid Elements", "There are no valid elements to export in this view");
                return;
            }

            ProgressBarWindow progressBar =
                ProgressBarWindow.Create(elementsInView.Count + 1, 0, "Converting elements...", this);

            // Use our custom implementation of IExportContext as the exporter context.
            GLTFExportContext ctx = new GLTFExportContext(doc);

            // Create a new custom exporter with the context.
            CustomExporter exporter = new CustomExporter(doc, ctx);
            exporter.ShouldStopOnError = false;

            #if REVIT2019
            exporter.Export(exportView);
            #else
            exporter.Export(exportView as View);
            #endif

            Thread.Sleep(1000);
            ProgressBarWindow.ViewModel.ProgressBarValue++;
            ProgressBarWindow.ViewModel.Message = "GLTF exportation completed!";
            ProgressBarWindow.ViewModel.Action = "Accept";
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

        private void RadioButtonFormatClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton button = sender as System.Windows.Controls.RadioButton;
            string value = button.Name;
            string key = "format";
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
