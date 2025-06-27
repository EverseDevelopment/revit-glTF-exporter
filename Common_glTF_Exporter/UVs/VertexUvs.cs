using Autodesk.Revit.DB;
using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Windows.MainWindow;
using Common_glTF_Exporter.Core;

namespace Common_glTF_Exporter.UVs
{
    public static class VertexUvs
    {
        public static void AddUvToVertex(XYZ vertex, GeometryDataObject geomItem, GLTFMaterial currentMaterial,
            Preferences preferences, Face currentFace)
        {
            if (preferences.materials == MaterialsEnum.textures &&
                currentMaterial?.EmbeddedTexturePath != null)
            {
                UV uv = null;

                if (uv == null && currentFace is CylindricalFace cf)
                {
                    uv = CylindricalUv.CalculateUv(cf, vertex);
                }

                if (uv == null && currentFace != null)
                {
                    uv = currentFace.Project(vertex)?.UVPoint;
                }

                if (uv == null && currentFace is PlanarFace pf)
                {
                    uv = PlanarUv.CalculateUv(pf, vertex);
                }

                if (uv == null)
                {
                    uv = new UV(vertex.X,
                                vertex.Y);
                }

                geomItem.Uvs.Add(uv);
            }
        }
    }
}
