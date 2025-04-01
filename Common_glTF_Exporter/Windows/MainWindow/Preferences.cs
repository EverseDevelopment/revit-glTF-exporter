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

    public enum FormatEnum
    {
        gltf,
        glb,
    }

    public enum MaterialsEnum
    {
        textures,
        materials,
        none
    }

    public class Preferences
    {
        public MaterialsEnum materials { get; set; }

        public FormatEnum format { get; set; }

        public bool normals { get; set; }

        public bool levels { get; set; }

        public bool grids { get; set; }

        public bool batchId { get; set; }

        public bool properties { get; set; }

        public bool relocateTo0 { get; set; }

        public bool flipAxis { get; set; }

        public CompressionEnum compression { get; set; }

        #if REVIT2019 || REVIT2020

        public DisplayUnitType units { get; set; }

        #else
        public ForgeTypeId units { get; set; }

        #endif

        public string path { get; set; }

        public string fileName { get; set; }
    }
}
