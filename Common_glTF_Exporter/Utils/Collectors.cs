namespace Common_glTF_Exporter.Utils
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;

    internal class Collectors
    {
        public static Material GetRandomMaterial(Document doc)
        {
            using (var collector = new FilteredElementCollector(doc))
            {
                return collector.OfCategory(BuiltInCategory.OST_Materials)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Material>()
                .FirstOrDefault();
            }
        }

        public static List<Element> AllVisibleElementsByView(Document doc, View view)
        {
            using (var collector = new FilteredElementCollector(doc, view.Id))
            {
                return collector.WhereElementIsNotElementType()
               .ToElements()
               .Cast<Element>()
               .Where(e => e.CanBeHidden(doc.ActiveView) && e.Category != null)
               .ToList();
            }
        }

        public static List<Element> AllVisibleElementsByViewRfa(Document doc, View view)
        {
            using (var collector = new FilteredElementCollector(doc, view.Id))
            {
                return collector.WhereElementIsNotElementType()
               .ToElements()
               .Cast<Element>()
               .Where(e => e.CanBeHidden(doc.ActiveView))
               .ToList();
            }
        }
    }
}
