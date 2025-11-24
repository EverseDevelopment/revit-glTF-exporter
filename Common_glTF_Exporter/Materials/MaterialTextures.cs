using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Model;
using glTF.Manipulator.GenericSchema;
using glTF.Manipulator.Schema;
using Revit_glTF_Exporter;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Interop;
using Material = Autodesk.Revit.DB.Material;

namespace Common_glTF_Exporter.Materials
{
    public static class MaterialTextures
    {
        
        public static (Color, Color) SetMaterialTextures(Material revitMaterial, BaseMaterial material,
           Document doc, float opacity, List<Texture> textures, List<glTFImage> images)
        {

            ElementId appearanceId = revitMaterial.AppearanceAssetId;
            if (appearanceId == ElementId.InvalidElementId)
            {
                return (null,null);
            }

            var appearanceElem = doc.GetElement(appearanceId) as AppearanceAssetElement;
            if (appearanceElem == null)
            {
                return (null, null);
            }

            Asset theAsset = appearanceElem.GetRenderingAsset();
            AssetPropertyString baseSchema = theAsset.FindByName("BaseSchema") as AssetPropertyString;
            if (baseSchema == null)
            {
                return (null, null);
            }

            string schemaName = baseSchema.Value;
            Asset connectedAsset = AssetPropertiesUtils.GetDiffuseBitmap(theAsset, schemaName);
            string texturePath = AssetPropertiesUtils.GetTexturePath(connectedAsset);
            Color tintColour = AssetPropertiesUtils.GetTint(theAsset);
            Color baseColor = AssetPropertiesUtils.GetAppearanceColor(theAsset, schemaName);
            double Fadevalue = AssetPropertiesUtils.GetFade(theAsset);

            if (!string.IsNullOrEmpty(texturePath))
            {
                int indexImage = createOrGetBaseImage(tintColour, baseColor, Fadevalue, texturePath, images);

                Texture baseTexture = createTexture(material, texturePath, connectedAsset, theAsset, opacity, indexImage);
                textures.Add(baseTexture);

                material.hasTexture = true;
                material.textureIndex = textures.Count - 1;
            }

            return (baseColor, tintColour);
        }

        private static Texture createTexture(BaseMaterial material, string texturePath, Asset connectedAsset,
           Asset theAsset, float opacity, int imageIndex)
        {

            Texture texture = new Texture();

            float scaleX = AssetPropertiesUtils.GetScale(connectedAsset, UnifiedBitmap.TextureRealWorldScaleX);
            float scaleY = AssetPropertiesUtils.GetScale(connectedAsset, UnifiedBitmap.TextureRealWorldScaleY);
            float offsetX = AssetPropertiesUtils.GetOffset(connectedAsset, UnifiedBitmap.TextureRealWorldOffsetX);
            float offsetY = AssetPropertiesUtils.GetOffset(connectedAsset, UnifiedBitmap.TextureRealWorldOffsetY);
            float rotation = AssetPropertiesUtils.GetRotationRadians(connectedAsset);

            float[] gltfScale = new float[] { 1f / scaleX, 1f / scaleY };
            float[] gltfOffset = new float[]
            {
                -(offsetX / scaleX),
                offsetY / scaleY - gltfScale[1]
            };

            material.offset = gltfOffset;
            material.rotation = rotation;
            material.scale = gltfScale;
            texture.source = imageIndex;

            return texture;
        }

        private static int createOrGetBaseImage(Color TintColour, Color BaseColor, double Fadevalue, string texturePath, List<glTFImage> images)
        {

            bool checkIfImageExists = images.Any(x => x.uuid == texturePath);

            if (checkIfImageExists)
            {
                int index = images.FindIndex(x => x.uuid == texturePath);

                return index;
            }
            else
            {
                glTFImage Image = new glTFImage();
                Image.uuid = texturePath;
                (string, ImageFormat) mimeType = BitmapsUtils.GetMimeType(texturePath);
                Image.mimeType = mimeType.Item1;
                byte[] imageBytes = BitmapsUtils.CleanGamma(texturePath, mimeType.Item2);
                byte[] blendedBytes = BitmapsUtils.BlendImageWithColor(imageBytes, Fadevalue,
                    BaseColor, mimeType.Item2, TintColour);
                Image.imageData = blendedBytes;
                images.Add(Image);
                int index = images.Count - 1;

                return index;
            }
        }
    }
}
