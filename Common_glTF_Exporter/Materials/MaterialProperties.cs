using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Common_glTF_Exporter.Core;

namespace Common_glTF_Exporter.Materials
{
    public static class MaterialProperties
    {
        private const string BLEND = "BLEND";
        private const string OPAQUE = "OPAQUE";
        public static void SetProperties(MaterialNode node, float opacity, ref GLTFPBR pbr, ref GLTFMaterial gl_mat)
        {
            pbr.metallicFactor = 0f;
            pbr.roughnessFactor = opacity != 1 ? 0.5f : 1f;
            gl_mat.pbrMetallicRoughness = pbr;

            gl_mat.alphaMode = opacity != 1 ? BLEND : OPAQUE;
            gl_mat.alphaCutoff = null;
        }

        public static void SetMaterialColour(MaterialNode node, 
            float opacity, ref GLTFPBR pbr, ref GLTFMaterial gl_mat)
        {
            if (gl_mat.EmbeddedTexturePath == null)
            {
                float sr = node.Color.Red / 255f;
                float sg = node.Color.Green / 255f;
                float sb = node.Color.Blue / 255f;

                // A linear conversion is needed to reflect the real colour
                float lr = SrgbToLinear(sr);
                float lg = SrgbToLinear(sg);
                float lb = SrgbToLinear(sb);

                pbr.baseColorFactor = new List<float>(4)
                {
                    lr,
                    lg,
                    lb,
                    opacity
                };
            }
            else
            {
                gl_mat.pbrMetallicRoughness.baseColorFactor = new List<float>(4)
                {
                    1,
                    1,
                    1,
                    opacity
                };
            }

            gl_mat.pbrMetallicRoughness = pbr;
        }

        public static float SrgbToLinear(float srgb)
        {
            return srgb <= 0.04045f
                ? srgb / 12.92f
                : (float)Math.Pow((srgb + 0.055f) / 1.055f, 2.4);  // System.Math
        }
    }
}
