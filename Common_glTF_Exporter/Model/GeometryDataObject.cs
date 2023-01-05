namespace Common_glTF_Exporter.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// Intermediate data format for converting between Revit Polymesh and glTF buffers.
    /// </summary>
    public class GeometryDataObject
    {
        public List<double> Vertices { get; set; } = new List<double>();

        public List<double> Normals { get; set; } = new List<double>();

        public List<double> Uvs { get; set; } = new List<double>();

        public List<int> Faces { get; set; } = new List<int>();
    }
}
