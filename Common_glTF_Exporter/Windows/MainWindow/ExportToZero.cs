namespace Common_glTF_Exporter.Windows.MainWindow
{
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Utils;
    using Revit_glTF_Exporter;

    public static class ExportToZero
    {
        public static XYZ GetPointToRelocate(Document doc)
        {
            Preferences preferences = Common_glTF_Exporter.Windows.MainWindow.Settings.GetInfo();

            if (preferences.relocateTo0)
            {
                var elementsOnActiveView = Collectors.AllVisibleElementsByView(doc, doc.ActiveView);
                var bb = Util.GetElementsBoundingBox(doc.ActiveView, elementsOnActiveView);
                return new XYZ((bb.Min.X / 2) + (bb.Max.X / 2), (bb.Min.Y / 2) + (bb.Max.Y / 2), (bb.Min.Z / 2) + (bb.Max.Z / 2));
            }

            return new XYZ(0, 0, 0);
        }
    }
}
