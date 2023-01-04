namespace Common_glTF_Exporter.Utils
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Windows.Media.Media3D;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Model;
    using Common_glTF_Exporter.Windows.MainWindow;
    using Revit_glTF_Exporter;

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

        public static GeometryElement GetGeometryElement(Document doc, Element element)
        {
            Options opt = new Options();
            opt.ComputeReferences = true;
            opt.View = doc.ActiveView;

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
