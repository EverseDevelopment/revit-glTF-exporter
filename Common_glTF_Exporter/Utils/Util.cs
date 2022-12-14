using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Autodesk.Revit.DB;
using Material = Autodesk.Revit.DB.Material;

namespace Revit_glTF_Exporter
{
    class Util
    {
        public static void SetAccuracy(Document doc, double decimalPlaces)
        {
            #if REVIT2019 || REVIT2020

            var _units = doc.GetUnits();
            var _length = _units.GetFormatOptions(UnitType.UT_Length);
            FormatOptions fo = new FormatOptions(_length);
            fo.UseDefault = false;
            //fo.Set(UnitTypeId.Meters);
            //fo.Accuracy = 1 * (10e-3);
            //var isValid = fo.IsValidAccuracy(fo.Accuracy);

            //if (isValid)
            //{
            //    _units.SetFormatOptions(SpecTypeId.Length, fo);
            //    doc.SetUnits(_units);
            //}

            #else

            var _units = doc.GetUnits();
            var _length = _units.GetFormatOptions(SpecTypeId.Length);
            FormatOptions fo = new FormatOptions(_length);
            fo.UseDefault = false;
            fo.SetUnitTypeId(UnitTypeId.Meters);
            fo.Accuracy = 1 * (10e-3);
            var isValid = fo.IsValidAccuracy(fo.Accuracy);

            if (isValid)
            {
                _units.SetFormatOptions(SpecTypeId.Length, fo);
                doc.SetUnits(_units);
            }

            #endif

        }



        public static glTFMaterial GetGLTFMaterial(List<glTFMaterial> glTFMaterials, Material material)
        {
            // search for an already existing material
            var m = glTFMaterials.FirstOrDefault(x =>
            x.pbrMetallicRoughness.baseColorFactor[0] == material.Color.Red &&
            x.pbrMetallicRoughness.baseColorFactor[1] == material.Color.Green &&
            x.pbrMetallicRoughness.baseColorFactor[2] == material.Color.Blue);

            return m != null ? m : Util.CreateGLTFMaterial("defaul", 50, new Color(250, 250, 250));
        }
        public static glTFMaterial CreateGLTFMaterial(string materialName, int materialOpacity, Color color)
        {
            // construct the material
            glTFMaterial gl_mat = new glTFMaterial();
            float opacity = 1 - (float)materialOpacity;
            gl_mat.name = materialName;
            glTFPBR pbr = new glTFPBR();
            pbr.baseColorFactor = new List<float>() { color.Red / 255f, color.Green / 255f, color.Blue / 255f, opacity };
            pbr.metallicFactor = 0f;
            pbr.roughnessFactor = 1f;
            gl_mat.pbrMetallicRoughness = pbr;

            return gl_mat;
        }
        public static Material GetMeshMaterial(Document doc, Mesh mesh)
        {
            ElementId materialId = mesh.MaterialElementId; 

            if (materialId != null)
            {
                return (doc.GetElement(materialId) as Material);
            }

            else { return null; }
        }
        /// <summary>
        /// Convert the given <paramref name="value"/> as a feet to the given <paramref name="forgeTypeId"/> unit.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="forgeTypeId"></param>
        /// <returns></returns>
        public static double ConvertFeetToUnitTypeId(double value,

#if REVIT2019 || REVIT2020

            DisplayUnitType displayUnitType

#else

            ForgeTypeId forgeTypeId

#endif

            )
        {
#if REVIT2019 || REVIT2020

            return UnitUtils.Convert(value, DisplayUnitType.DUT_DECIMAL_FEET, displayUnitType);
            
#else

            return UnitUtils.Convert(value, UnitTypeId.Feet, forgeTypeId);

#endif
        }
        public static float[] GetVec3MinMax(List<float> vec3)
        {
            
            List<float> xValues = new List<float>();
            List<float> yValues = new List<float>();
            List<float> zValues = new List<float>();
            for (int i = 0; i < vec3.Count; i++)
            {
                if ((i % 3) == 0) xValues.Add(vec3[i]);
                if ((i % 3) == 1) yValues.Add(vec3[i]);
                if ((i % 3) == 2) zValues.Add(vec3[i]);
            }

            float maxX = xValues.Max();
            float minX = xValues.Min();
            float maxY = yValues.Max();
            float minY = yValues.Min();
            float maxZ = zValues.Max();
            float minZ = zValues.Min();

            return new float[] { minX, maxX, minY, maxY, minZ, maxZ };
        }

        public static int[] GetScalarMinMax(List<int> scalars)
        {
            int minFaceIndex = int.MaxValue;
            int maxFaceIndex = int.MinValue;
            for (int i = 0; i < scalars.Count; i++)
            {
                int currentMin = Math.Min(minFaceIndex, scalars[i]);
                if (currentMin < minFaceIndex) minFaceIndex = currentMin;

                int currentMax = Math.Max(maxFaceIndex, scalars[i]);
                if (currentMax > maxFaceIndex) maxFaceIndex = currentMax;
            }
            return new int[] { minFaceIndex, maxFaceIndex };
        }

        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return a string for a real number
        /// formatted to two decimal places.
        /// </summary>
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
        public static string PointString(XYZ p)
        {
            return string.Format("({0},{1},{2})",
              RealString(p.X),
              RealString(p.Y),
              RealString(p.Z));
        }

        /// <summary>
        /// From Jeremy Tammik's RvtVa3c exporter:
        /// https://github.com/va3c/RvtVa3c
        /// Return an integer value for a Revit Color.
        /// </summary>
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
        public static string ElementDescription(Element e)
        {
            if (null == e)
            {
                return "<null>";
            }

            // For a wall, the element name equals the
            // wall type name, which is equivalent to the
            // family name ...

            FamilyInstance fi = e as FamilyInstance;

            string typeName = e.GetType().Name;

            string categoryName = (null == e.Category)
              ? string.Empty
              : e.Category.Name + " ";

            string familyName = (null == fi)
              ? string.Empty
              : fi.Symbol.Family.Name + " ";

            string symbolName = (null == fi
              || e.Name.Equals(fi.Symbol.Name))
                ? string.Empty
                : fi.Symbol.Name + " ";

            return string.Format("{0} {1}{2}{3}<{4} {5}>",
              typeName, categoryName, familyName,
              symbolName, e.Id.IntegerValue, e.Name);
        }

        /// <summary>
        /// Gets a list of "Project UUID" values corresponding to 
        /// an element's dependent (hosted) elements
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static List<string> GetDependentElements(Element e)
        {

            IList<ElementId> dependentElements = e.GetDependentElements(null);

            List<string> dependentElementUuids = new List<string>();

            Document doc = e.Document;

            foreach (ElementId elId in dependentElements)
            {
                if (elId != e.Id)
                {
                    Element dependentElement = doc.GetElement(elId);
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
        public static Dictionary<string, string> GetElementParameters(Element e, bool includeType)
        {
            IList<Parameter> parameters
              = e.GetOrderedParameters();

            Dictionary<string, string> a = new Dictionary<string, string>(parameters.Count);

            // Add element category
            //a.Add("Element Category", e.Category.Name);

            string key;
            string val;

            foreach (Parameter p in parameters)
            {
                key = p.Definition.Name;

                if (!a.ContainsKey(key))
                {
                    if (StorageType.String == p.StorageType)
                    {
                        val = p.AsString();
                    }
                    else
                    {
                        val = p.AsValueString();
                    }
                    if (!string.IsNullOrEmpty(val))
                    {
                        a.Add(key, val);
                    }
                }
            }

            if (includeType)
            {
                ElementId idType = e.GetTypeId();

                if (ElementId.InvalidElementId != idType)
                {
                    Document doc = e.Document;
                    Element typ = doc.GetElement(idType);
                    parameters = typ.GetOrderedParameters();
                    foreach (Parameter p in parameters)
                    {
                        key = "Type " + p.Definition.Name;

                        if (!a.ContainsKey(key))
                        {
                            if (StorageType.String == p.StorageType)
                            {
                                val = p.AsString();
                            }
                            else
                            {
                                val = p.AsValueString();
                            }
                            if (!string.IsNullOrEmpty(val))
                            {
                                a.Add(key, val);
                            }
                        }
                    }
                }
            }
            return a;
        }
    }
}
