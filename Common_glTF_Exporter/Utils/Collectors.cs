namespace Common_glTF_Exporter.Utils
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;

    internal class Collectors
    {
        public static Material GetRandomMaterial(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Materials)
                .WhereElementIsNotElementType()
                .ToElements()
                .Cast<Material>()
                .FirstOrDefault();
        }

        public static List<Element> AllVisibleElementsByView(Document doc, View view)
        {
            return new FilteredElementCollector(doc, view.Id)
               .WhereElementIsNotElementType()
               .ToElements()
               .Cast<Element>()
               .Where(e => e.CanBeHidden(doc.ActiveView) && e.CanBeLocked())
               .ToList();
        }
    }
}
