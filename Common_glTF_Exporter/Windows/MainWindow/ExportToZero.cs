namespace Common_glTF_Exporter.Windows.MainWindow
{
    using System.Collections.Generic;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Utils;
    using Revit_glTF_Exporter;

    public static class ExportToZero
    {
        public static List<double> GetPointToRelocate(Document doc, double scale)
        {
            Preferences preferences = Common_glTF_Exporter.Windows.MainWindow.Settings.GetInfo();

            if (preferences.relocateTo0)
            {
                var elementsOnActiveView = Collectors.AllVisibleElementsByView(doc, doc.ActiveView);
                var bb = Util.GetElementsBoundingBox(doc.ActiveView, elementsOnActiveView);

                double pointX = -scale * ((bb.Min.X / 2) + (bb.Max.X / 2));
                double pointy = -scale * ((bb.Min.Z / 2) + (bb.Max.Z / 2));
                double pointz = -scale * ((bb.Min.Y / 2) + (bb.Max.Y / 2));

                return new List<double> { pointX, pointy, pointz };
            }

            return new List<double> { 0, 0, 0 };
        }
    }
}
