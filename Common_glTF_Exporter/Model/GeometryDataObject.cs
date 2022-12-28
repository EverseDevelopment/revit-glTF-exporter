using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Model
{
    /// <summary>
    /// Intermediate data format for 
    /// converting between Revit Polymesh
    /// and glTF buffers.
    /// </summary>
    public class GeometryDataObject
    {
        public List<double> vertices = new List<double>();
        public List<double> normals = new List<double>();
        public List<double> uvs = new List<double>();
        public List<int> faces = new List<int>();
    }
}
