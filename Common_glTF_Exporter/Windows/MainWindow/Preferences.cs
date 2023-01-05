namespace Common_glTF_Exporter.Windows.MainWindow
{
    using Autodesk.Revit.DB;

    public enum CompressionEnum
    {
        None,
        Meshopt,
        Draco,
        ZIP,
    }

    public class Preferences
    {
        public bool materials { get; set; }

        public bool elementId { get; set; }

        public bool normals { get; set; }

        public bool levels { get; set; }

        public bool lights { get; set; }

        public bool grids { get; set; }

        public bool batchId { get; set; }

        public bool properties { get; set; }

        public bool relocateTo0 { get; set; }

        public bool flipAxis { get; set; }

        public bool singleBinary { get; set; }

        public CompressionEnum compression { get; set; }

        #if REVIT2019 || REVIT2020

        public DisplayUnitType units { get; set; }

        #else
        public ForgeTypeId units { get; set; }

        #endif
        public int digits { get; set; }

        public string path { get; set; }

        public string fileName { get; set; }
    }
}
