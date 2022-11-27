using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using Revit_glTF_Exporter.Model;

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
            SaveFileDialog fileDialog = new SaveFileDialog();

            fileDialog.FileName = _fileName + " - " + _viewName; // default file name
            fileDialog.DefaultExt = ".gltf"; // default file extension

            bool? dialogResult = fileDialog.ShowDialog();
            if (dialogResult == true)
            {
                string filename = fileDialog.FileName;
                string directory = System.IO.Path.GetDirectoryName(filename) + "\\";

                ExportView3D(_view, filename, directory, false);
            }
        }

        public void ExportView3D(View3D view3d, string filename, string directory, bool mode)
        {

            Document doc = view3d.Document;

            // Use our custom implementation of IExportContext as the exporter context.
            glTFExportContext ctx = new glTFExportContext(doc, filename , directory + "\\");
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
