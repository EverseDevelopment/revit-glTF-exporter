﻿using Autodesk.Revit.DB;
using Revit_glTF_Exporter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Model
{
    /// <summary>
    /// From Jeremy Tammik's RvtVa3c exporter:
    /// https://github.com/va3c/RvtVa3c
    /// An integer-based 3D point class.
    /// </summary>
    class PointIntObject : IComparable<PointIntObject>
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        private double _boundingBoxMidPointX { get; set; }
        private double _boundingBoxMidPointY { get; set; }
        private double _boundingBoxMidPointZ { get; set; }

        public PointIntObject(XYZ p, bool switch_coordinates,

            #if REVIT2019 || REVIT2020

            DisplayUnitType displayUnitType,

            #else

            ForgeTypeId forgeTypeId,

            #endif

            bool relocateTo0, XYZ pointToRelocate)
        {
            #if REVIT2019 || REVIT2020

            X = Util.ConvertFeetToUnitTypeId(p.X, displayUnitType);
            Y = Util.ConvertFeetToUnitTypeId(p.Y, displayUnitType);
            Z = Util.ConvertFeetToUnitTypeId(p.Z, displayUnitType);

            if (relocateTo0)
            {
                _boundingBoxMidPointX = Util.ConvertFeetToUnitTypeId(pointToRelocate.X, displayUnitType);
                _boundingBoxMidPointY = Util.ConvertFeetToUnitTypeId(pointToRelocate.Y, displayUnitType);
                _boundingBoxMidPointZ = Util.ConvertFeetToUnitTypeId(pointToRelocate.Z, displayUnitType);
            }

            #else

            X = Util.ConvertFeetToUnitTypeId(p.X, forgeTypeId);
            Y = Util.ConvertFeetToUnitTypeId(p.Y, forgeTypeId);
            Z = Util.ConvertFeetToUnitTypeId(p.Z, forgeTypeId);

            if (relocateTo0)
	        {
                _boundingBoxMidPointX = Util.ConvertFeetToUnitTypeId(p.X, forgeTypeId);
                _boundingBoxMidPointY = Util.ConvertFeetToUnitTypeId(p.Y, forgeTypeId);
                _boundingBoxMidPointZ = Util.ConvertFeetToUnitTypeId(p.Z, forgeTypeId);
	        }

            #endif

            if (relocateTo0)
            {
                pointToRelocate = new XYZ(_boundingBoxMidPointX, _boundingBoxMidPointY, _boundingBoxMidPointZ);

                X -= pointToRelocate.X;
                Y -= pointToRelocate.Y;
                Z -= pointToRelocate.Z;
            }

            if (switch_coordinates)
            {
                X = -X;
                double tmp = Y;
                Y = Z;
                Z = tmp;
            }
        }

        public int CompareTo(PointIntObject a)
        {
            double d = X - a.X;
            if (0 == d)
            {
                d = Y - a.Y;
                if (0 == d)
                {
                    d = Z - a.Z;
                }
            }
            return (0 == d) ? 0 : ((0 < d) ? 1 : -1);
        }
    }
}