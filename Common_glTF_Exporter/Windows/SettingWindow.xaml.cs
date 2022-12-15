using System.Windows;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Common_glTF_Exporter.ViewModel;
using Common_glTF_Exporter.Utils;
using System.IO;

namespace Revit_glTF_Exporter
{
    public partial class SettingWindow : Window
    {
        public static SettingWindow MainView { get; set; } = new SettingWindow();
        public SettingWindow()
        {
            MainView = this;
            InitializeComponent();
            SetDefaultSettings();
        }

        public static void SetDefaultSettings()
        {
            //Exporting
            MainView.ExportLights_Checkbox.IsChecked = false;
            MainView.ExportBatchId_CheckBox.IsChecked = true;
            MainView.ExportNormals_CheckBox.IsChecked = false;
            MainView.ExportGrids_CheckBox.IsChecked = true;
            MainView.ExportLevels_CheckBox.IsChecked = true;
            MainView.ExportBoundingBox_CheckBox.IsChecked = true;

            //Position
            MainView.RelocateModel_CheckBox.IsChecked = false;
            MainView.FlipAxys_Checkbox.IsChecked = true;

            //Compression
            MainView.NoneCompression_RadioButton.IsChecked = true;
            MainView.MeshoptCompression_RadioButton.IsChecked = false;
            MainView.DRACOCompression_RadioButton.IsChecked = false;
            MainView.ZIPCompression_RadioButton.IsChecked = false;

            //Accuracy
            MainView.Accuracy_Slider.Value= 3;
        }
        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
