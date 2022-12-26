using System;
using System.Collections.Generic;
using System.Security.RightsManagement;
using System.Text;

namespace Common_glTF_Exporter.Windows.MainWindow
{
    public class Preferences
    {
        public bool materials { get; set; }
        public bool elementId { get; set; }
        public bool normals { get; set; }
        public bool levels   { get; set; }
        public bool lights { get; set; }
        public bool grids { get; set; }
        public bool batchId { get; set; }
        public bool boundingBox { get; set; }
        public bool relocateTo0 { get; set; }
        public bool flipAxis { get; set; }
        public bool exportProperties { get; set; }
        public bool singleBinary { get; set; }
        public string units { get; set; }
        public CompressionEnum compression { get; set; }
        public string digits { get; set; }
        public string path { get; set; }
        public string fileName { get; set; }
    }
    public enum CompressionEnum
    {
        None,
        Meshopt,
        Draco,
        ZIP
    }
}
