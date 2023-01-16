namespace Revit_glTF_Exporter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Windows.MainWindow;

    public class Util
    {
        /// <summary>
        /// Analyze if the given <paramref name="element"/> can be locked OR hidden.
        /// </summary>
        /// <param name="element">Revit element.</param>
        /// <param name="view">Revit view.</param>
        /// <returns>If the given element can't be locked OR can't be hidden, it will returns FALSE. Otherwise, will returns TRUE.</returns>
        public static bool CanBeLockOrHidden(Element element, View view)
        {
            if (!element.CanBeHidden(view))
            {
                return false;
            }

            return true;
        }

        public static BoundingBoxXYZ GetElementsBoundingBox(View view, List<Element> elements)
        {
            // Get the bounding box of the visible elements
            List<XYZ> maxPoints = new List<XYZ>();
            List<XYZ> minPoints = new List<XYZ>();
            List<string> categories = new List<string>();

            foreach (Element element in elements)
            {
                BoundingBoxXYZ elementBoundingBox = element.get_BoundingBox(view);

                if (elementBoundingBox == null)
                {
                    continue;
                }

                if (element.CanBeHidden(view))
                {
                    maxPoints.Add(elementBoundingBox.Max);
                    minPoints.Add(elementBoundingBox.Min);
                }
            }

            XYZ maxPoint = new XYZ(maxPoints.Max(x => x.X), maxPoints.Max(x => x.Y), maxPoints.Max(x => x.Z));
            XYZ minPoint = new XYZ(minPoints.Min(x => x.X), minPoints.Min(x => x.Y), minPoints.Min(x => x.Z));
            BoundingBoxXYZ newBB = new BoundingBoxXYZ();
            newBB.Max = maxPoint;
            newBB.Min = minPoint;

            return newBB;
        }

        /// <summary>
        /// Convert the given <paramref name="value"/> as a feet to the given <paramref name="forgeTypeId"/> unit.
        /// </summary>
        /// <param name="preferences">User preferences.</param>
        /// <returns>Converted value.</returns>
        public static double ConvertFeetToUnitTypeId(Preferences preferences)
        {
            #if REVIT2019 || REVIT2020

            return Math.Round(UnitUtils.Convert(1, DisplayUnitType.DUT_DECIMAL_FEET, preferences.units), preferences.digits);

            #else

            return Math.Round(UnitUtils.Convert(1, UnitTypeId.Feet, preferences.units), preferences.digits);

            #endif
        }

        public static float[] GetVec3MinMax(List<float> vec3)
        {
            List<float> xvalues = new List<float>();
            List<float> yvalues = new List<float>();
            List<float> zvalues = new List<float>();
            for (int i = 0; i < vec3.Count; i++)
            {
                if ((i % 3) == 0)
                {
                    xvalues.Add(vec3[i]);
                }

                if ((i % 3) == 1)
                {
                    yvalues.Add(vec3[i]);
                }

                if ((i % 3) == 2)
                {
                    zvalues.Add(vec3[i]);
                }
            }

            float maxX = xvalues.Max();
            float minX = xvalues.Min();
            float maxY = yvalues.Max();
            float minY = yvalues.Min();
            float maxZ = zvalues.Max();
            float minZ = zvalues.Min();

            return new float[] { minX, maxX, minY, maxY, minZ, maxZ };
        }

        public static int[] GetScalarMinMax(List<int> scalars)
        {
            int minFaceIndex = int.MaxValue;
            int maxFaceIndex = int.MinValue;
            for (int i = 0; i < scalars.Count; i++)
            {
                int currentMin = Math.Min(minFaceIndex, scalars[i]);
                if (currentMin < minFaceIndex)
                {
                    minFaceIndex = currentMin;
                }

                int currentMax = Math.Max(maxFaceIndex, scalars[i]);
                if (currentMax > maxFaceIndex)
                {
                    maxFaceIndex = currentMax;
                }
            }

            return new int[] { minFaceIndex, maxFaceIndex };
        }

        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return a string for a real number
        /// formatted to two decimal places.
        /// </summary>
        /// <returns>Real number with two decimal places.</returns>
        /// <param name="a">Number to be converted to string.</param>
        public static string RealString(double a)
        {
            return a.ToString("0.##");
        }

        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return a string for an XYZ point
        /// or vector with its coordinates
        /// formatted to two decimal places.
        /// </summary>
        /// <returns>A string that represents a XYZ point coordinates.</returns>
        /// <param name="p">XYZ point.</param>
        public static string PointString(XYZ p)
        {
            return string.Format(
              "({0},{1},{2})",
              RealString(p.X),
              RealString(p.Y),
              RealString(p.Z));
        }

        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return an integer value for a Revit Color.
        /// </summary>
        /// <returns>An int that represents a RGB color.</returns>
        /// <param name="color">Color.</param>
        public static int ColorToInt(Color color)
        {
            return ((int)color.Red) << 16
              | ((int)color.Green) << 8
              | (int)color.Blue;
        }

        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return a string describing the given element:
        /// .NET type name,
        /// category name,
        /// family and symbol name for a family instance,
        /// element id and element name.
        /// </summary>
        /// <returns>The element description.</returns>
        /// <param name="e">Revit element.</param>
        public static string ElementDescription(Element e)
        {
            if (e == null)
            {
                return "<null>";
            }

            // For a wall, the element name equals the wall type name, which is equivalent to the family name ...
            FamilyInstance fi = e as FamilyInstance;

            string typeName = e.GetType().Name;

            string categoryName = (e.Category == null)
              ? string.Empty
              : string.Concat(e.Category.Name, " ");

            string familyName = (fi == null)
              ? string.Empty
              : string.Concat(fi.Symbol.Family.Name, " ");

            string symbolName = (fi == null
              || e.Name.Equals(fi.Symbol.Name))
                ? string.Empty
                : string.Concat(fi.Symbol.Name, " ");

            return string.Format(
              "{0} {1}{2}{3}<{4} {5}>",
              typeName,
              categoryName,
              familyName,
              symbolName,
              e.Id.IntegerValue,
              e.Name);
        }

        /// <summary>
        /// Gets a list of "Project UUID" values corresponding to
        /// an element's dependent (hosted) elements.
        /// </summary>
        /// <param name="element">Revit element.</param>
        /// <returns>List of dependent element Project UUID values.</returns>
        public static List<string> GetDependentElements(Element element)
        {
            IList<ElementId> dependentElements = element.GetDependentElements(null);

            List<string> dependentElementUuids = new List<string>();

            Document doc = element.Document;

            foreach (ElementId elementId in dependentElements)
            {
                if (elementId != element.Id)
                {
                    Element dependentElement = doc.GetElement(elementId);
                    string uuid = dependentElement.LookupParameter("ProjectUUID")?.AsString();
                    if (uuid != null)
                    {
                        dependentElementUuids.Add(uuid);
                    }
                }
            }

            return dependentElementUuids;
        }

        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return a dictionary of all the given
        /// element parameter names and values.
        /// </summary>
        /// <param name="element">Revit element.</param>
        /// <param name="includeType">Include element information.</param>
        /// <returns>Element parameters dictionary.</returns>
        public static Dictionary<string, string> GetElementParameters(Element element, bool includeType)
        {
            IList<Parameter> parameters = element.GetOrderedParameters();

            Dictionary<string, string> parametersDictionary = new Dictionary<string, string>(parameters.Count);

            string key;
            string val;

            foreach (Parameter parameter in parameters)
            {
                key = parameter.Definition.Name;

                if (!parametersDictionary.ContainsKey(key))
                {
                    if (parameter.StorageType == StorageType.String)
                    {
                        val = parameter.AsString();
                    }
                    else
                    {
                        val = parameter.AsValueString();
                    }

                    if (!string.IsNullOrEmpty(val))
                    {
                        parametersDictionary.Add(key, val);
                    }
                }
            }

            if (includeType)
            {
                ElementId elementId = element.GetTypeId();

                if (ElementId.InvalidElementId != elementId)
                {
                    Document doc = element.Document;
                    Element typ = doc.GetElement(elementId);
                    parameters = typ.GetOrderedParameters();
                    foreach (Parameter parameter in parameters)
                    {
                        key = string.Concat("Type ", parameter.Definition.Name);

                        if (!parametersDictionary.ContainsKey(key))
                        {
                            if (parameter.StorageType == StorageType.String)
                            {
                                val = parameter.AsString();
                            }
                            else
                            {
                                val = parameter.AsValueString();
                            }

                            if (!string.IsNullOrEmpty(val))
                            {
                                parametersDictionary.Add(key, val);
                            }
                        }
                    }
                }
            }

            return parametersDictionary;
        }
    }
}
