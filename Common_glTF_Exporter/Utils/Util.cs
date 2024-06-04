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
        /// <returns>
        /// If the given element can't be locked OR can't be hidden, it will returns FALSE.
        /// Otherwise, will returns TRUE.
        /// </returns>
        public static bool CanBeLockOrHidden(Element element, View view, bool rfaFile)
        {
            if (!rfaFile && element.Category.CanAddSubcategory)
            {
                return true;
            }
            if (element.CanBeHidden(view))
            {
                return true;
            }

            return false;
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

        public static float[] GetVec3MinMax(IEnumerable<float> vec3)
        {
            var xvalues = vec3.Where((i, j) => j % 3 == 0);
            var yvalues = vec3.Where((i, j) => j % 3 == 1);
            var zvalues = vec3.Where((i, j) => j % 3 == 2);

            return new float[] { xvalues.Min(), xvalues.Max(), yvalues.Min(), yvalues.Max(), zvalues.Min(), zvalues.Max() };
        }

        public static int[] GetScalarMinMax(List<int> scalars)
        {
            if (scalars == null || scalars.Count == 0) return null;
            return new int[] { scalars.Min(), scalars.Max() };
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

        static StringBuilder ElementDescriptionStrBuilder = new StringBuilder();

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
            ElementDescriptionStrBuilder.Clear();

            if (e == null)
            {
                return NullStr;
            }

            // For a wall, the element name equals the wall type name, which is equivalent to the family name ...
            FamilyInstance fi = e as FamilyInstance;

            ElementDescriptionStrBuilder.Append(e.GetType().Name);
            ElementDescriptionStrBuilder.Append(SpaceStr);

            if (e.Category != null)
            {
                ElementDescriptionStrBuilder.Append(e.Category.Name);
                ElementDescriptionStrBuilder.Append(SpaceStr);
            }

            if (fi != null)
            {
                ElementDescriptionStrBuilder.Append(fi.Symbol.Family.Name);
                ElementDescriptionStrBuilder.Append(SpaceStr);

                if (!e.Name.Equals(fi.Symbol.Name))
                {
                    ElementDescriptionStrBuilder.Append(fi.Symbol.Name);
                    ElementDescriptionStrBuilder.Append(SpaceStr);
                }
            }

            ElementDescriptionStrBuilder.Append(LessSignStr);
            ElementDescriptionStrBuilder.Append(e.Id.IntegerValue);
            ElementDescriptionStrBuilder.Append(SpaceStr);
            ElementDescriptionStrBuilder.Append(e.Name);
            ElementDescriptionStrBuilder.Append(GreaterSignStr);

            return ElementDescriptionStrBuilder.ToString();
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

            var parametersDictionary = new Dictionary<string, string>(parameters.Count());
            var keys = new HashSet<string>();

            var sb = new StringBuilder();

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
                var elementId = element.GetTypeId();

                if (ElementId.InvalidElementId != elementId)
                {
                    var doc = element.Document;
                    var elementType = doc.GetElement(elementId);

                    parameters = elementType.GetOrderedParameters();

                    foreach (Parameter parameter in parameters)
                    {
                        sb.Clear();
                        sb.Append(TypeStr).Append(parameter.Definition.Name);
                        key = sb.ToString();

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
