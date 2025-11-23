namespace Common_glTF_Exporter.Model
{
    using Autodesk.Revit.DB;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Intermediate data format for converting between Revit Polymesh and glTF buffers.
    /// </summary>
    public class GeometryDataObject
    {
        public List<double> Vertices { get; set; } = new List<double>();

        public List<double> Normals { get; set; } = new List<double>();

        public List<GltfUV> Uvs { get; set; } = new List<GltfUV>();

        public List<int> Faces { get; set; } = new List<int>();

        public MaterialInfo MaterialInfo { get; set; }

        /// <summary>
        /// Creates a deep clone of this GeometryDataObject.
        /// </summary>
        public GeometryDataObject Clone()
        {
            return new GeometryDataObject
            {
                Vertices = new List<double>(this.Vertices),
                Normals = new List<double>(this.Normals),
                Faces = new List<int>(this.Faces),

                Uvs = this.Uvs
                    .Select(uv => new GltfUV { U = uv.U, V = uv.V })
                    .ToList(),

                MaterialInfo = this.MaterialInfo != null
                    ? new MaterialInfo { uuid = this.MaterialInfo.uuid }
                    : null
            };
        }
    }

    public class MaterialInfo
    {
        public string uuid { get; set; }
    }


    public class GltfUV
    {
        public float U { get; set; }
        public float V { get; set; }
    }
}
