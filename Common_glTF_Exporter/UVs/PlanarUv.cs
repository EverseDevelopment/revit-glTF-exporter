using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.UVs
{
    public static class PlanarUv
    {
        public static UV CalculateUv(PlanarFace face, XYZ vertex)
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
