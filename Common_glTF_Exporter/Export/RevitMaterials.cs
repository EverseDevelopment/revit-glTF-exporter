namespace Common_glTF_Exporter.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows.Forms;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.DB.Visual;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Windows.MainWindow;
    using Revit_glTF_Exporter;

    public static class RevitMaterials
    {
        const string BLEND = "BLEND";
        const string OPAQUE = "OPAQUE";
        const int ONEINTVALUE = 1;

        /// <summary>
        /// Container for material names (Local cache to avoid Revit API I/O)
        /// </summary>
        static Dictionary<ElementId, MaterialCacheDTO> MaterialNameContainer = new Dictionary<ElementId, MaterialCacheDTO>();

        /// <summary>
        /// Export Revit materials.
        /// </summary>
        public static GLTFMaterial Export(MaterialNode node,
            ref IndexedDictionary<GLTFMaterial> materials,
            Preferences preferences)
        {
            ElementId id = node.MaterialId;
            GLTFMaterial gl_mat = new GLTFMaterial();
            float opacity = ONEINTVALUE - (float)node.Transparency;

            Document doc = ExternalApplication.RevitCollectorService.GetDocument();

            if (id != ElementId.InvalidElementId)
            {
                string uniqueId;
                Material material = null;

                if (!MaterialNameContainer.TryGetValue(node.MaterialId, out var materialElement))
                {
                    material = doc.GetElement(node.MaterialId) as Material;
                    gl_mat.name = material.Name;
                    uniqueId = material.UniqueId;
                    MaterialNameContainer.Add(node.MaterialId, new MaterialCacheDTO(material.Name, material.UniqueId));
                }
                else
                {
                    var elementData = MaterialNameContainer[node.MaterialId];
                    gl_mat.name = elementData.MaterialName;
                    uniqueId = elementData.UniqueId;
                    material = doc.GetElement(node.MaterialId) as Material;
                }

                // Set PBR values
                GLTFPBR pbr = new GLTFPBR();
                SetMaterialsProperties(node, opacity, ref pbr, ref gl_mat);

                // Instead of embedding the image now, just store the path for future export
                if (material != null && preferences.materials == MaterialsEnum.textures)
                {

                    Asset connectedAsset = TryGetConnectedAsset(material, doc);
                    string texturePath = TryGetTexturePath(connectedAsset);


                    if (!string.IsNullOrEmpty(texturePath) && File.Exists(texturePath))
                    {
                        gl_mat.EmbeddedTexturePath = texturePath;

                        float scaleX = GetScale(connectedAsset, "texture_RealWorldScaleX");
                        float scaleY = GetScale(connectedAsset, "texture_RealWorldScaleY");

                        gl_mat.pbrMetallicRoughness.baseColorTexture = new GLTFTextureInfo
                        {
                            index = -1, // This will be correctly updated in `AddGeometryMeta`
                            extensions = new GLTFTextureExtensions
                            {
                                TextureTransform = new GLTFTextureTransform
                                {
                                    scale = new float[] { 1f / scaleX, 1f / scaleY } // Or whatever scale fits your model's original UVs
                                }
                            }
                        };
                    }
                }

                materials.AddOrUpdateCurrentMaterial(uniqueId, gl_mat, false);
            }

            return gl_mat;
        }

        private static void SetMaterialsProperties(MaterialNode node, float opacity, ref GLTFPBR pbr, ref GLTFMaterial gl_mat)
        {
            pbr.baseColorFactor = new List<float>(4)
            {
                node.Color.Red / 255f,
                node.Color.Green / 255f,
                node.Color.Blue / 255f,
                opacity
            };

            pbr.metallicFactor = 0f;
            pbr.roughnessFactor = opacity != 1 ? 0.5f : 1f;
            gl_mat.pbrMetallicRoughness = pbr;

            gl_mat.alphaMode = opacity != 1 ? BLEND : OPAQUE;
            gl_mat.alphaCutoff = null;
        }

        /// <summary>
        /// Extracts the texture path from the material’s AppearanceAsset, if present.
        /// </summary>
        private static Asset TryGetConnectedAsset(Material material, Document doc)
        {
            ElementId appearanceId = material.AppearanceAssetId;
            if (appearanceId == ElementId.InvalidElementId)
                return null;

            var appearanceElem = doc.GetElement(appearanceId) as AppearanceAssetElement;
            if (appearanceElem == null)
                return null;

            Asset theAsset = appearanceElem.GetRenderingAsset();
            AssetProperty prop = theAsset.FindByName("opaque_albedo");

            if (prop != null && prop.NumberOfConnectedProperties > 0)
            {
                Asset connectedAsset = prop.GetSingleConnectedAsset();
                return connectedAsset;
            }

            return null;
        }

        private static string TryGetTexturePath(Asset connectedAsset)
        {
            if(connectedAsset != null)
            {
                var bitmapPathProp = connectedAsset.FindByName("unifiedbitmap_Bitmap") as AssetPropertyString;

                if (bitmapPathProp != null)
                {
                    string texturePath = bitmapPathProp.Value.Split('|')[0].Replace("/", "\\");

                    if (!Path.IsPathRooted(texturePath))
                    {
                        string materialsPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles),
                            @"Autodesk Shared\Materials\Textures\");
                        texturePath = Path.Combine(materialsPath, texturePath);
                    }

                    return texturePath;
                }
            }

            return null;
        }

        private static float GetScale(Asset connectedAsset, string textureName)
        {
            AssetPropertyDistance scale = connectedAsset.FindByName(textureName) as AssetPropertyDistance;

            if (scale != null)
            {
                return (float)scale.Value;
            }

            return 1;
        }
    }




    public class MaterialCacheDTO
    {
        public MaterialCacheDTO(string materialName, string uniqueId)
        {
            MaterialName = materialName;
            UniqueId = uniqueId;
        }

        public string MaterialName { get; set; }
        public string UniqueId { get; set; }
    }
}
