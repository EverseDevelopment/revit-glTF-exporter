namespace Common_glTF_Exporter.Utils
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;

    public class GeometryUtils
    {
        public static List<Mesh> GetMeshes(Document doc, Element element)
        {
            var geoEle = GetGeometryElement(doc, element);

            List<Mesh> meshes = new List<Mesh>();
            foreach (GeometryInstance geoObject in geoEle.OfType<GeometryInstance>())
            {
                foreach (var geoObj in geoObject.GetInstanceGeometry().OfType<Mesh>())
                {
                    Mesh mesh = geoObj;
                    meshes.Add(mesh);
                }
            }

            return meshes;
        }

        /// <summary>
        /// Get the GeometryElement leveraging preset options to Compute References on Active View.
        /// </summary>
        /// <param name="doc">Revit Document.</param>
        /// <param name="element">Revit Element.</param>
        /// <returns>The GeometryElement.</returns>
        public static GeometryElement GetGeometryElement(Document doc, Element element)
        {
            GeometryElement result;
            try
            {
                Options opt = new Options
                {
                    ComputeReferences = true,
                    View = doc.ActiveView,
                };
                result = element.get_Geometry(opt);
            }
            catch
            {
                Options opt = new Options
                {
                    ComputeReferences = true,
                };
                result = element.get_Geometry(opt);
            }

            return result;
        }


        public static XYZ GetNormal(MeshTriangle triangle)
        {
            var vertex0 = triangle.get_Vertex(0);
            XYZ side1 = triangle.get_Vertex(1) - vertex0;
            XYZ side2 = triangle.get_Vertex(2) - vertex0;
            XYZ normal = side1.CrossProduct(side2);
            return normal.Normalize();
        }
    }
}
