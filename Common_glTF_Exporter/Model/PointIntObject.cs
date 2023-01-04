namespace Common_glTF_Exporter.Model
{
    using System;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Windows.MainWindow;
    using Revit_glTF_Exporter;

    /// <summary>
    /// From Jeremy Tammik's RvtVa3c exporter:
    /// https://github.com/va3c/RvtVa3c
    /// An integer-based 3D point class.
    /// </summary>
    public class PointIntObject : IComparable<PointIntObject>
    {
        public PointIntObject(Preferences preferences, XYZ p, XYZ pointToRelocate)
        {
            this.X = Util.ConvertFeetToUnitTypeId(p.X, preferences);
            this.Y = Util.ConvertFeetToUnitTypeId(p.Y, preferences);
            this.Z = Util.ConvertFeetToUnitTypeId(p.Z, preferences);

            if (preferences.relocateTo0)
            {
                this.BoundingBoxMidPointX = Util.ConvertFeetToUnitTypeId(pointToRelocate.X, preferences);
                this.BoundingBoxMidPointY = Util.ConvertFeetToUnitTypeId(pointToRelocate.Y, preferences);
                this.BoundingBoxMidPointZ = Util.ConvertFeetToUnitTypeId(pointToRelocate.Z, preferences);

                pointToRelocate = new XYZ(this.BoundingBoxMidPointX, this.BoundingBoxMidPointY, this.BoundingBoxMidPointZ);

                this.X -= pointToRelocate.X;
                this.Y -= pointToRelocate.Y;
                this.Z -= pointToRelocate.Z;
            }

            if (preferences.flipAxis)
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
