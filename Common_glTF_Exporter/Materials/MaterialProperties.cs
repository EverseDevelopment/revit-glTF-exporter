using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Xml.Linq;
using Autodesk.Revit.DB;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Model;
using glTF.Manipulator.GenericSchema;
using glTF.Manipulator.Schema;

namespace Common_glTF_Exporter.Materials
{
    public static class MaterialProperties
    {
        private const string BLEND = "BLEND";
        private const string OPAQUE = "OPAQUE";
        public static void SetProperties(MaterialNode node, float opacity, ref BaseMaterial material)
        {
            material.metallicFactor = 0f;
            material.roughnessFactor = opacity != 1 ? 0.5f : 1f;
            material.alphaMode = opacity != 1 ? BLEND : OPAQUE;
            material.alphaCutoff = null;
        }

        public static List<float> SetMaterialColour(MaterialNode node,
            float opacity, Autodesk.Revit.DB.Color baseColor, Autodesk.Revit.DB.Color tintColor)
        {

                (float, float, float) baseColours;

                if (baseColor == null)
                {
                    baseColours = RgbToUnit(node.Color);
                }
                else
                {
                    baseColours = RgbToUnit(baseColor);
                }

                if (tintColor == null)
                {
                   return GetLinearColour(baseColours, opacity);
                }
                else
                {
                    (float, float, float) baseTintColour = RgbToUnit(tintColor);
                    (float, float, float) blendColour = BlendColour(baseColours, baseTintColour);

                    return GetLinearColour(blendColour, opacity);
                }
        }


        public static List<float> GetDefaultColour(float opacity)
        {
            return new List<float>(4) { 1, 1, 1, opacity };
        }

        public static (float, float, float) BlendColour((float, float, float) colourA, 
            (float, float, float) colourB)
        {
            float lr = colourA.Item1 * colourB.Item1;
            float lg = colourA.Item2 * colourB.Item2;
            float lb = colourA.Item3 * colourB.Item3;

            return (lr, lg, lb);
        }

        public static List<float> GetLinearColour((float, float, float) baseColours, float opacity)
        {
            float lr = SrgbToLinear(baseColours.Item1);
            float lg = SrgbToLinear(baseColours.Item2);
            float lb = SrgbToLinear(baseColours.Item3);

            return new List<float>(4){ lr, lg, lb, opacity};
        }

        public static (float, float, float) RgbToUnit(Autodesk.Revit.DB.Color color)
        {
            float sr = color.Red / 255f;
            float sg = color.Green / 255f;
            float sb = color.Blue / 255f;

            return (sr, sg, sb);

        }

        public static float SrgbToLinear(float srgb)
        {
            return srgb <= 0.04045f
                ? srgb / 12.92f
                : (float)Math.Pow((srgb + 0.055f) / 1.055f, 2.4);  // System.Math
        }
    }
}
