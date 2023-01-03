namespace Revit_glTF_Exporter.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Autodesk.Revit.DB;

    public class Room
    {
        public string Name { get; set; }

        public Element Element { get; set; }

        public ElementId ElementId { get; set; }

        public List<ElementId> ElementList { get; set; }
    }
}
