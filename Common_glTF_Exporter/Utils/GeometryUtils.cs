namespace Common_glTF_Exporter.Utils
{
    using System.Collections.Generic;
    using Autodesk.Revit.DB;

    public class GeometryUtils
    {
        public static List<Mesh> GetMeshes(Document doc, Element element)
        {
            var geoEle = GetGeometryElement(doc, element);

            List<Mesh> meshes = new List<Mesh>();

            foreach (GeometryObject geoObject in geoEle)
            {
                if (geoObject is GeometryInstance)
                {
                    GeometryInstance geoInst = geoObject as GeometryInstance;

                    foreach (var geoObj in geoInst.GetInstanceGeometry())
                    {
                        if (geoObj is Mesh)
                        {
                            Mesh mesh = geoObj as Mesh;
                            meshes.Add(mesh);
                        }
                    }
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
            Options opt = new Options
            {
                ComputeReferences = true,
                View = doc.ActiveView,
            };

            return element.get_Geometry(opt);
        }

        public static XYZ GetNormal(MeshTriangle triangle)
        {
            XYZ side1 = triangle.get_Vertex(1) - triangle.get_Vertex(0);
            XYZ side2 = triangle.get_Vertex(2) - triangle.get_Vertex(0);
            XYZ normal = side1.CrossProduct(side2);
            return normal.Normalize();
        }
    }
}
