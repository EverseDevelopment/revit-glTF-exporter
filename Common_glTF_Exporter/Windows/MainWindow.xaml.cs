using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Autodesk.Internal.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Common_glTF_Exporter;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Utils;
using Common_glTF_Exporter.ViewModel;
using Common_glTF_Exporter.Windows.MainWindow;
using Theme = Common_glTF_Exporter.Utils.Theme;
using View = Autodesk.Revit.DB.View;
using Common_glTF_Exporter.Materials;

namespace Revit_glTF_Exporter
{
    /// <summary>
    /// Interaction logic for Settings.xaml.
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private Document doc;
        public static List<string> TexturePaths { get; set; }

        public MainWindow(View view)
        {
            this.UnitsViewModel = new UnitsViewModel();
            this.DataContext = this.UnitsViewModel;
            MainView = this;
            doc = ExternalApplication.RevitCollectorService.GetDocument();

            this.InitializeComponent();

            ComboUnits.Set(doc);
            this.View = view;

            UpdateForm.Run(this.MainWindow_Border);
            LabelVersion.Update(this.UnitsViewModel);

            Theme.ApplyDarkLightMode(this.Resources.MergedDictionaries[0]);

            TexturePaths = TextureLocation.GetPaths();

            Analytics.Send("Open", "Main Window").GetAwaiter();
            ExportLog.StartLog();
            ExportLog.Write("Open Window");
        }

        public static MainWindow MainView { get; set; }

        private View View { get; set; }

        private UnitsViewModel UnitsViewModel { get; set; }

        private void OnExportView(object sender, RoutedEventArgs e)
        {
            string format = string.Concat(".", SettingsConfig.GetValue("format"));
            LogConfiguration.SaveConfig();
            View3D exportView = this.View as View3D;
            
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

            List<Element> elementsInView = Collectors.AllVisibleElementsByView(doc, doc.ActiveView);

            if (!doc.IsFamilyDocument && !elementsInView.Any())
            {
                MessageWindow.Show("No Valid Elements", "There are no valid elements to export in this view");
                ExportLog.Write("There are no valid elements to export in this view");
                return;
            }

            int numberRuns = int.Parse(SettingsConfig.GetValue("runs"));
            int incrementRun = numberRuns + 1;
            SettingsConfig.SetValue("runs", incrementRun.ToString());

            ExportLog.Write($"{elementsInView.Count} elements will be esported");
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

            Analytics.Send("exported", SettingsConfig.GetValue("format")).GetAwaiter();
            Thread.Sleep(500);
            ProgressBarWindow.ViewModel.ProgressBarValue = elementsInView.Count + 1;
            ProgressBarWindow.ViewModel.ProgressBarPercentage = 100;
            ProgressBarWindow.ViewModel.Message = "Export completed!";
            ExportLog.EndLog();
            ProgressBarWindow.ViewModel.Action = "Accept";
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

        private void Leia_Link(object sender, RoutedEventArgs e)
        {
            Hyperlink.Run(Links.leiaWebsite);
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

        private void RadioButtonMaterialsClick(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton button = sender as System.Windows.Controls.RadioButton;
            string value = button.Name;
            string key = "materials";
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
