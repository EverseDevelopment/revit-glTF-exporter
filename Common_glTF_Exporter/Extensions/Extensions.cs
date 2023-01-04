﻿namespace Common_glTF_Exporter.Extensions
{
    using Autodesk.Revit.DB;

    public static class Extensions
    {
        public static XYZ FlipCoordinates(this XYZ point)
        {
            double x = -point.X;
            double y = point.Z;
            double z = point.Y;

            return new XYZ(x, y, z);
        }
    }
}