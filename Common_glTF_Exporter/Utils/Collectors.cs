using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common_glTF_Exporter.Utils
{
    class Collectors
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
        public static List<Element> AllElementsByView(Document doc, View view)
        {
            return new FilteredElementCollector(doc, view.Id)
               .WhereElementIsNotElementType()
               .ToElements()
               .ToList();
        }
    }
}
