namespace Revit_glTF_Exporter.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Autodesk.Revit.DB;

    public interface IObject
    {
        Category Category { get; set; }

        string FamilySymbol { get; set; }

        string ElementName { get; set; }

        ElementId EId { get; set; }

        Location Location { get; set; }
    }
}
