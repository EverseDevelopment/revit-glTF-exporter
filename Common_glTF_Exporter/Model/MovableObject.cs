using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace Revit_glTF_Exporter.Model
{
    public class MovableObject : IObject
    {
        public Category Category { get; set; }

        public string FamilySymbol { get; set; }

        public string ElementName { get; set ; }

        public ElementId EId { get; set; }

        public Location Location { get; set; }

        public ElementId RoomId { get; set; }
    }
}
