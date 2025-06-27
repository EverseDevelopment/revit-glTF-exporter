using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.UVs
{
    public static class CylindricalUv
    {
        public static UV CalculateUv(CylindricalFace face, XYZ p)
        {
            var surf = face.GetSurface() as CylindricalSurface
                       ?? throw new ArgumentException(
                           "Face is not cylindrical.", nameof(face));

            XYZ o = surf.Origin;
            XYZ z = surf.Axis.Normalize();
            XYZ x = surf.XDir.Normalize();
            XYZ y = surf.YDir.Normalize();
            double r = surf.Radius;

            XYZ vOP = p - o;
            double vParam = vOP.DotProduct(z);

            XYZ radial = vOP - vParam * z;
            if (radial.IsZeroLength())
                return new UV(0, vParam);

            radial = radial.Normalize();

            double cosT = radial.DotProduct(x);
            double sinT = radial.DotProduct(y);
            double uRad = Math.Atan2(sinT, cosT);
            if (uRad < 0) uRad += 2 * Math.PI;

            if (!surf.OrientationMatchesParametricOrientation)
                vParam = -vParam;

            return new UV(uRad, vParam);
        }
    }
}
