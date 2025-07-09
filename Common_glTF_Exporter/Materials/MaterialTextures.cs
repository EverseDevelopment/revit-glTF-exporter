using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Windows.MainWindow;
using Revit_glTF_Exporter;
using Common_glTF_Exporter.Materials;
using Common_glTF_Exporter.Model;
using System.IO.Ports;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using Material = Autodesk.Revit.DB.Material;
using System;

namespace Common_glTF_Exporter.Materials
{
    public static class MaterialTextures
    {
        const string REALWORLDSCALEX = "texture_RealWorldScaleX";
        const string REALWORLDSCALEY = "texture_RealWorldScaleY";
        const string GENERICDIFFUSEFADE = "generic_diffuse_image_fade";
        const string GENERICTINT = "common_Tint_color";
        const string COMMONTINTTOGGLE = "common_Tint_toggle";

        public static GLTFMaterial SetMaterialTextures(Material material, GLTFMaterial gl_mat,
    Document doc, float opacity)
        {

            ElementId appearanceId = material.AppearanceAssetId;
            if (appearanceId == ElementId.InvalidElementId)
            {
                return gl_mat;
            }

            var appearanceElem = doc.GetElement(appearanceId) as AppearanceAssetElement;
            if (appearanceElem == null)
            {
                return gl_mat;
            }

            Asset theAsset = appearanceElem.GetRenderingAsset();

            Asset connectedAsset = AssetPropertiesUtils.GetDiffuseBitmap(theAsset);
            string texturePath = AssetPropertiesUtils.GetTexturePath(connectedAsset);


            if (!string.IsNullOrEmpty(texturePath) && File.Exists(texturePath))
            {
                SetTextureProperties(gl_mat, texturePath, connectedAsset, theAsset, opacity);
            }

            return gl_mat;
        }

        private static void SetTextureProperties(GLTFMaterial gl_mat, string texturePath, Asset connectedAsset,
           Asset theAsset, float opacity)
        {
            gl_mat.EmbeddedTexturePath = texturePath;

            float scaleX = AssetPropertiesUtils.GetScale(connectedAsset, REALWORLDSCALEX);
            float scaleY = AssetPropertiesUtils.GetScale(connectedAsset, REALWORLDSCALEY);

            float rotation = AssetPropertiesUtils.GetRotationRadians(connectedAsset);

            AssetPropertyDouble fadeProp = theAsset.FindByName(GENERICDIFFUSEFADE) as AssetPropertyDouble;

            if (fadeProp != null)
            {
                gl_mat.Fadevalue = fadeProp.Value;
            }

            bool tintOn = true;

            AssetProperty tintEnabledProp = theAsset.FindByName(COMMONTINTTOGGLE);
            if (tintEnabledProp is AssetPropertyBoolean apb)
            {
                tintOn = apb.Value;
            }

            if (tintOn)
            {
                AssetProperty tintProp = theAsset.FindByName(GENERICTINT);
                if (tintProp is AssetPropertyDoubleArray4d tintArray4d)
                {
                    IList<double> rgba = tintArray4d.GetValueAsDoubles();

                    byte r = (byte)(rgba[0] * 255.0);
                    byte g = (byte)(rgba[1] * 255.0);
                    byte b = (byte)(rgba[2] * 255.0);

                    gl_mat.TintColour = new Autodesk.Revit.DB.Color(r, g, b);
                }
            }

            if (fadeProp != null)
            {
                gl_mat.Fadevalue = fadeProp.Value;
            }

            gl_mat.BaseColor = AssetPropertiesUtils.GetAppearenceColor(theAsset);
            gl_mat.pbrMetallicRoughness.baseColorFactor = new List<float>(4)
            {
                1,
                1,
                1,
                opacity
            };

            gl_mat.pbrMetallicRoughness.baseColorTexture = new GLTFTextureInfo
            {
                index = -1,
                extensions = new GLTFTextureExtensions
                {
                    TextureTransform = new GLTFTextureTransform
                    {
                        scale = new float[] { 1f / scaleX, 1f / scaleY },
                        rotation = rotation
                    }
                }
            };
        }
    }
}
