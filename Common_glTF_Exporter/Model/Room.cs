namespace Revit_glTF_Exporter.Model
{
    using System.Collections.Generic;
    using Autodesk.Revit.DB;

    public class Room
    {
        public string Name { get; set; }

        public Element Element { get; set; }

        public ElementId ElementId { get; set; }

        public List<ElementId> ElementList { get; set; }
    }
}
