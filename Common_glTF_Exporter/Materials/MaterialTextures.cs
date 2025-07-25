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
using Common_glTF_Exporter.Utils;
using System.Diagnostics;

namespace Common_glTF_Exporter.Materials
{
    public static class MaterialTextures
    {
        
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
            AssetPropertyString baseSchema = theAsset.FindByName("BaseSchema") as AssetPropertyString;
            if (baseSchema == null)
            {
                return gl_mat;
            }

            string schemaName = baseSchema.Value;
            Asset connectedAsset = AssetPropertiesUtils.GetDiffuseBitmap(theAsset, schemaName);
            string texturePath = AssetPropertiesUtils.GetTexturePath(connectedAsset);
            gl_mat.TintColour = AssetPropertiesUtils.GetTint(theAsset);
            gl_mat.BaseColor = AssetPropertiesUtils.GetAppearanceColor(theAsset, schemaName);

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

            float scaleX = AssetPropertiesUtils.GetScale(connectedAsset, UnifiedBitmap.TextureRealWorldScaleX);
            float scaleY = AssetPropertiesUtils.GetScale(connectedAsset, UnifiedBitmap.TextureRealWorldScaleY);
            float offsetX = AssetPropertiesUtils.GetOffset(connectedAsset, UnifiedBitmap.TextureRealWorldOffsetX);
            float offsetY = AssetPropertiesUtils.GetOffset(connectedAsset, UnifiedBitmap.TextureRealWorldOffsetY);
            float rotation = AssetPropertiesUtils.GetRotationRadians(connectedAsset);

            gl_mat.Fadevalue = AssetPropertiesUtils.GetFade(theAsset);         

            float[] gltfScale = new float[] { 1f / scaleX, 1f / scaleY };
            float[] gltfOffset = new float[]
            {
                -(offsetX / scaleX),
                offsetY / scaleY - gltfScale[1]
            };

            gl_mat.pbrMetallicRoughness.baseColorTexture = new GLTFTextureInfo
            {
                index = -1,
                extensions = new GLTFTextureExtensions
                {
                    TextureTransform = new GLTFTextureTransform
                    {
                        offset = gltfOffset,
                        scale = gltfScale,
                        rotation = rotation
                    }
                }
            };
        }
    }
}
