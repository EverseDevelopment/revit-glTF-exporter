using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Common_glTF_Exporter.Export;
using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Transform;
using Common_glTF_Exporter.Utils;
using Common_glTF_Exporter.Windows.MainWindow;
using Revit_glTF_Exporter;
using Transform = Autodesk.Revit.DB.Transform;
using Common_glTF_Exporter.EportUtils;
using System.Windows.Media.Media3D;
using System.Windows.Controls;
using Common_glTF_Exporter.UVs;
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

                if (currentFace != null)
                {
                    uv = currentFace.Project(vertex)?.UVPoint;
                }

                if (uv == null && currentFace is PlanarFace pf)
                {
                    uv = GetPlanarUv(pf, vertex);
                }

                if (uv == null)
                {
                    const double ftToM = 0.3048;   
                    double tile = 1.0;      
                    uv = new UV(vertex.X * ftToM / tile,
                                vertex.Y * ftToM / tile);
                }

                geomItem.Uvs.Add(uv);
            }
        }

        public static UV GetPlanarUv(PlanarFace face, XYZ vertex)
        {
            XYZ origin = face.Origin;
            XYZ uDir = face.XVector.Normalize();
            XYZ vDir = face.YVector.Normalize();

            XYZ delta = vertex - origin;

            double u = delta.DotProduct(uDir);
            double v = delta.DotProduct(vDir);

            return new UV(u, v);
        }
    }
}
