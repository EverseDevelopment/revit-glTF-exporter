namespace Revit_glTF_Exporter
{
    using System;
    using System.IO;
    using Autodesk.Revit.ApplicationServices;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Common_glTF_Exporter.Utils;

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiapp = commandData.Application;
                UIDocument uidoc = uiapp.ActiveUIDocument;
                Application app = uiapp.Application;
                Document doc = uidoc.Document;

                View view = doc.ActiveView;

                SettingsConfig.SetValue("user", app.Username);
                SettingsConfig.SetValue("release", app.VersionName);

                if (view.GetType().Name != "View3D")
                {
                    MessageWindow.Show("Wrong View", "You must be in a 3D view to export");
                    return Result.Succeeded;
                }

                MainWindow mainWindow = new MainWindow(doc, view);
                mainWindow.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Analytics.Send("Error", ex.Message).GetAwaiter();
                MessageWindow.Show("Error", ex.Message);
                return Result.Failed;
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class Debug : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var uiapp = commandData.Application;
                var uidoc = uiapp.ActiveUIDocument;
                var app = uiapp.Application;
                var doc = uidoc.Document;

                var view = doc.ActiveView;

                if (view.GetType().Name != "View3D")
                {
                    TaskDialog.Show("Wrong View", "You must be in a 3D view to export");
                    return Result.Succeeded;
                }

                var programDataLocation = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                var appSettingsFile = string.Concat(programDataLocation, "\\Autodesk\\ApplicationPlugins\\leia.bundle\\Contents\\2025\\Leia_glTF_Exporter.dll.config");

                if (!File.Exists(appSettingsFile))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(appSettingsFile));
                    File.WriteAllText(appSettingsFile, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<configuration />");
                }

                SettingsConfig.SetValue("materials", "true");
                SettingsConfig.SetValue("format", "glb");
                SettingsConfig.SetValue("normals", "true");
                SettingsConfig.SetValue("levels", "false");
                SettingsConfig.SetValue("lights", "false");
                SettingsConfig.SetValue("grids", "false");
                SettingsConfig.SetValue("batchId", "false");
                SettingsConfig.SetValue("properties", "true");
                SettingsConfig.SetValue("relocateTo0", "true");
                SettingsConfig.SetValue("flipAxis", "true");
                SettingsConfig.SetValue("units", "autodesk.unit.unit:meters-1.0.0");
                SettingsConfig.SetValue("compression", "Meshopt");
                SettingsConfig.SetValue("path", Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\out");
                SettingsConfig.SetValue("fileName", "out");
                SettingsConfig.SetValue("user", app.Username);
                SettingsConfig.SetValue("release", app.VersionName);
                SettingsConfig.SetValue("isRFA", "false");                

                var ctx = new GLTFExportContext(doc, true);
                var exporter = new CustomExporter(doc, ctx)
                {
                    ShouldStopOnError = false
                };
                exporter.Export(view);

                TaskDialog.Show("glTF Export", "Finished");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.ToString());
                return Result.Failed;
            }
        }
    }

    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class AboutUs : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                var mainWindow = new AboutUsWindow();
                mainWindow.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Analytics.Send("Error", ex.Message).GetAwaiter();
                MessageWindow.Show("Error", ex.Message);
                return Result.Failed;
            }
        }
    }
}