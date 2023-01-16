namespace Common_glTF_Exporter.Windows.MainWindow
{
    using System.Collections.Generic;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Utils;
    using Revit_glTF_Exporter;

    public static class ExportToZero
    {
        public static List<float> GetPointToRelocate(Document doc, double scale, bool flip)
        {
            Preferences preferences = Common_glTF_Exporter.Windows.MainWindow.Settings.GetInfo();

            if (preferences.relocateTo0)
            {
                var elementsOnActiveView = Collectors.AllVisibleElementsByView(doc, doc.ActiveView);
                var bb = Util.GetElementsBoundingBox(doc.ActiveView, elementsOnActiveView);

                double pointX = -scale * ((bb.Min.X + bb.Max.X) / 2);
                double pointy = -scale * ((bb.Min.Z + bb.Max.Z) / 2);
                double pointz = -scale * ((bb.Min.Y + bb.Max.Y) / 2);
                if (flip)
                {
                    return new List<float> { (float)pointX, (float)pointy, -(float)pointz };
                }
                else
                {
                    return new List<float> { (float)pointX, (float)pointz, (float)pointy };
                }
            }

            return new List<float> { 0, 0, 0 };
        }
    }
}
