namespace Common_glTF_Exporter.Core
{
    public class MaterialTexture
    {
        public string EmbeddedTexturePath { get; set; } = null;
        public double Fadevalue { get; set; } = 1;

        public Autodesk.Revit.DB.Color TintColour { get; set; }

        public Autodesk.Revit.DB.Color BaseColor { get; set; }
    }
}
