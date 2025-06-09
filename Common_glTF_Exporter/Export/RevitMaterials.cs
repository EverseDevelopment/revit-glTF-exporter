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
            Preferences preferences, Document doc)
        {
            ElementId id = node.MaterialId;
            GLTFMaterial gl_mat = new GLTFMaterial();
            float opacity = ONEINTVALUE - (float)node.Transparency;

            if (id != ElementId.InvalidElementId)
            {
                Material material = null;

                if (!MaterialNameContainer.TryGetValue(node.MaterialId, out var materialElement))
                {
                    material = doc.GetElement(node.MaterialId) as Material;

                    if (material == null)
                    {
                        return gl_mat;
                    }

                    gl_mat.name = material.Name;
                    gl_mat.UniqueId = material.UniqueId;
                    MaterialNameContainer.Add(node.MaterialId, new MaterialCacheDTO(material.Name, material.UniqueId));
                }
                else
                {
                    var elementData = MaterialNameContainer[node.MaterialId];
                    gl_mat.name = elementData.MaterialName;
                    gl_mat.UniqueId = elementData.UniqueId;
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

                        float rotation = GetRotationRadians(connectedAsset);

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
            else 
            {
                gl_mat.name = "default";
                gl_mat.UniqueId = Guid.NewGuid().ToString();
                GLTFPBR pbr = new GLTFPBR();
                pbr.baseColorFactor = new List<float>(4)
                {
                    1,
                    1,
                    1,
                    opacity
                };

                pbr.metallicFactor = 0f;
                pbr.roughnessFactor = opacity != 1 ? 0.5f : 1f;
                gl_mat.pbrMetallicRoughness = pbr;

                gl_mat.alphaMode = opacity != 1 ? BLEND : OPAQUE;
                gl_mat.alphaCutoff = null;
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
        /// Tries "opaque_albedo" first, then iterates over all properties to find a connected asset.
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

            // First try "opaque_albedo"
            AssetProperty prop = theAsset.FindByName("opaque_albedo");
            if (prop != null && prop.NumberOfConnectedProperties > 0)
            {
                Asset connectedAsset = prop.GetSingleConnectedAsset();
                if (connectedAsset != null)
                    return connectedAsset;
            }

            // Fallback: search all properties for first connected asset
            for (int i = 0; i < theAsset.Size; i++)
            {
                var ap = theAsset[i];
                if (ap.NumberOfConnectedProperties == 1)
                {
                    var connectedAsset = ap.GetSingleConnectedAsset();
                    if (connectedAsset != null)
                        return connectedAsset;
                }
            }

            return null;
        }

        private static string TryGetTexturePath(Asset connectedAsset)
        {
            if(connectedAsset != null)
            {
                var bitmapPathProp = connectedAsset.FindByName("unifiedbitmap_Bitmap") as AssetPropertyString;

                if (bitmapPathProp != null && !string.IsNullOrEmpty(bitmapPathProp.Value))
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

        private static float GetRotationRadians(Asset connectedAsset)
        {
            // In Revit, rotation is usually in degrees
            AssetPropertyDouble rotation = connectedAsset.FindByName("texture_WAngle") as AssetPropertyDouble;

            if (rotation != null)
            {
                // Convert degrees to radians for glTF
                return (float)(rotation.Value * Math.PI / 180.0);
            }

            return 0f;
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
