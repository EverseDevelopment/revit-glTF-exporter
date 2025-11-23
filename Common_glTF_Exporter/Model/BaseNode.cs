using glTF.Manipulator.Schema;
using Revit_glTF_Exporter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Model
{
    public class BaseNode
    {
        public string description { get; set; }
        public string name { get; set; }

        public string uuid { get; set; }

        public long id { get; set; }
        public Extras extras { get; set; }
        public IndexedDictionary<GeometryDataObject> objects { get; set; }
    }
}
