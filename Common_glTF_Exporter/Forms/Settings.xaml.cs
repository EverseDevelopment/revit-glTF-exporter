using System.Windows;
using System.Windows.Controls;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Common_glTF_Exporter.Utils;
using Microsoft.Win32;

namespace Revit_glTF_Exporter
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    
    public partial class Settings : Window
    {
        Document _doc;
        View3D _view;
        string _fileName;
        string _viewName;
        //public bool materialsExport = true;
        public Settings(Document doc, View3D view)
        {
            InitializeComponent();

            this._doc = doc;
            this._view = view;
            this._fileName = _doc.Title;
            this._viewName = _view.Name;
        }

        private void OnExportView(object sender, RoutedEventArgs e)
        {
            if (_view == null)
            {
                TaskDialog.Show("glTFRevitExport", "You must be in a 3D view to export.");
                this.Close();
            }

            string fileName = string.Concat(_fileName, " - ", _viewName);
            bool dialogResult = FilesHelper.AskToSave(ref fileName, string.Empty, ".gltf");

            if (dialogResult == true)
            {
                string filename = fileName;
                string directory = System.IO.Path.GetDirectoryName(filename) + "\\";

                ExportView3D(_view, filename, directory, false);
            }
        }

        public void ExportView3D(View3D view3d, string filename, string directory, bool mode)
        {
            Document doc = view3d.Document;

            // Use our custom implementation of IExportContext as the exporter context.
            glTFExportContext ctx = new glTFExportContext(doc, filename , directory + "\\", true, true, 
                FlipAxysCheckbox.IsChecked.Value, MaterialsCheckbox.IsChecked.Value);

            // Create a new custom exporter with the context.
            CustomExporter exporter = new CustomExporter(doc, ctx);
                
            exporter.ShouldStopOnError = true;
            exporter.Export(view3d);            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
