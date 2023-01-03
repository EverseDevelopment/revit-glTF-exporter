namespace Common_glTF_Exporter.Model
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Autodesk.Revit.DB;
    using Revit_glTF_Exporter;

    /// <summary>
    /// From Jeremy Tammik's RvtVa3c exporter:
    /// https://github.com/va3c/RvtVa3c
    /// An integer-based 3D point class.
    /// </summary>
    public class PointIntObject : IComparable<PointIntObject>
    {
        public PointIntObject(
        XYZ p,
        bool switch_coordinates,
        #if REVIT2019 || REVIT2020
        DisplayUnitType displayUnitType,
        #else
        ForgeTypeId forgeTypeId,
        #endif
        bool relocateTo0,
        XYZ pointToRelocate,
        int decimalPlaces)
        {
            #if REVIT2019 || REVIT2020

            this.X = Util.ConvertFeetToUnitTypeId(p.X, displayUnitType, decimalPlaces);
            this.Y = Util.ConvertFeetToUnitTypeId(p.Y, displayUnitType, decimalPlaces);
            this.Z = Util.ConvertFeetToUnitTypeId(p.Z, displayUnitType, decimalPlaces);

            if (relocateTo0)
            {
                this.BoundingBoxMidPointX = Util.ConvertFeetToUnitTypeId(pointToRelocate.X, displayUnitType, decimalPlaces);
                this.BoundingBoxMidPointY = Util.ConvertFeetToUnitTypeId(pointToRelocate.Y, displayUnitType, decimalPlaces);
                this.BoundingBoxMidPointZ = Util.ConvertFeetToUnitTypeId(pointToRelocate.Z, displayUnitType, decimalPlaces);
            }

            #else

            this.X = Util.ConvertFeetToUnitTypeId(p.X, forgeTypeId, decimalPlaces);
            this.Y = Util.ConvertFeetToUnitTypeId(p.Y, forgeTypeId, decimalPlaces);
            this.Z = Util.ConvertFeetToUnitTypeId(p.Z, forgeTypeId, decimalPlaces);

            if (relocateTo0)
            {
                this.BoundingBoxMidPointX = Util.ConvertFeetToUnitTypeId(p.X, forgeTypeId, decimalPlaces);
                this.BoundingBoxMidPointY = Util.ConvertFeetToUnitTypeId(p.Y, forgeTypeId, decimalPlaces);
                this.BoundingBoxMidPointZ = Util.ConvertFeetToUnitTypeId(p.Z, forgeTypeId, decimalPlaces);
            }

            #endif

            if (relocateTo0)
            {
                pointToRelocate = new XYZ(this.BoundingBoxMidPointX, this.BoundingBoxMidPointY, this.BoundingBoxMidPointZ);

                this.X -= pointToRelocate.X;
                this.Y -= pointToRelocate.Y;
                this.Z -= pointToRelocate.Z;
            }

            if (switch_coordinates)
            {
                this.X = -this.X;
                double tmp = this.Y;
                this.Y = this.Z;
                this.Z = tmp;
            }
        }

        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        private double BoundingBoxMidPointX { get; set; }

        private double BoundingBoxMidPointY { get; set; }

        private double BoundingBoxMidPointZ { get; set; }

        public int CompareTo(PointIntObject a)
        {
            double d = this.X - a.X;
            if (d == 0)
            {
                d = this.Y - a.Y;
                if (d == 0)
                {
                    d = this.Z - a.Z;
                }
            }

            return (d == 0) ? 0 : ((d > 0) ? 1 : -1);
        }
    }
}
