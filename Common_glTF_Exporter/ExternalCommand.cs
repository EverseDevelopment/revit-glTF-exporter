namespace Revit_glTF_Exporter
{
    using Autodesk.Revit.ApplicationServices;
    using Autodesk.Revit.Attributes;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using System;
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

                MainWindow mainWindow = new MainWindow(view);
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