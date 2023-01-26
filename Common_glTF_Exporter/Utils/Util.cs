namespace Revit_glTF_Exporter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
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

            return UnitUtils.Convert(1, DisplayUnitType.DUT_DECIMAL_FEET, preferences.units);

            #else

            return UnitUtils.Convert(1, UnitTypeId.Feet, preferences.units);

            #endif
        }

        public static float[] GetVec3MinMax(List<float> vec3)
        {
            var xvalues = vec3.Where((i, j) => j % 3 == 0).ToList();
            var yvalues = vec3.Where((i, j) => j % 3 == 1).ToList();
            var zvalues = vec3.Where((i, j) => j % 3 == 2).ToList();

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

        const string SpaceStr = " ";
        const string NullStr = "<null>";
        const string LessSignStr = "<";
        const string GreaterSignStr = "<";

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
                return NullStr;
            }

            // For a wall, the element name equals the wall type name, which is equivalent to the family name ...
            FamilyInstance fi = e as FamilyInstance;
            StringBuilder sb = new StringBuilder();

            sb.Append(e.GetType().Name);
            sb.Append(SpaceStr);

            if (e.Category != null)
            {
                sb.Append(e.Category.Name);
                sb.Append(SpaceStr);
            }

            if (fi != null)
            {
                sb.Append(fi.Symbol.Family.Name);
                sb.Append(SpaceStr);

                if (!e.Name.Equals(fi.Symbol.Name))
                {
                    sb.Append(fi.Symbol.Name);
                    sb.Append(SpaceStr);
                }
            }

            sb.Append(LessSignStr);
            sb.Append(e.Id.IntegerValue);
            sb.Append(SpaceStr);
            sb.Append(e.Name);
            sb.Append(GreaterSignStr);

            return sb.ToString();
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

        const string TypeStr = "Type ";

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
            IEnumerable<Parameter> parameters = element.GetOrderedParameters();

            Dictionary<string, string> parametersDictionary = new Dictionary<string, string>(parameters.Count());
            HashSet<string> keys = new HashSet<string>();

            string key;
            string val;

            foreach (Parameter parameter in parameters)
            {
                key = parameter.Definition.Name;

                if (!keys.Contains(key))
                {
                    keys.Add(key);
                    val = GetParameterValue(parameter);

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
                        key = string.Concat(TypeStr, parameter.Definition.Name);

                        if (!parametersDictionary.ContainsKey(key))
                        {
                            val = GetParameterValue(parameter);
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

        private static string GetParameterValue(Parameter parameter)
        {
            if (parameter.StorageType == StorageType.String)
            {
                return parameter.AsString();
            }
            else
            {
                return parameter.AsValueString();
            }
        }
    }
}
