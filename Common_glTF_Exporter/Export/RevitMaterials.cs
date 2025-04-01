namespace Common_glTF_Exporter.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.DB.Visual;
    using Common_glTF_Exporter.Core;
    using Revit_glTF_Exporter;

    public static class RevitMaterials
    {
        const string BLEND = "BLEND";
        const string OPAQUE = "OPAQUE";
        const int ONEINTVALUE = 1;
        const string GENERIC_DIFFUSE = "GenericDiffuse";
        const string UNIFIED_BITMAP = "UnifiedBitmap";

        /// <summary>
        /// Container for material names (Local cache to avoid Revit API I/O)
        /// </summary>
        static Dictionary<ElementId, MaterialCacheDTO> MaterialNameContainer = new Dictionary<ElementId, MaterialCacheDTO>();

        /// <summary>
        /// Export Revit materials.
        /// </summary>
        /// <param name="node">node.</param>
        /// <param name="doc">Revit document.</param>
        /// <param name="materials">Materials.</param>
        /// <param name="textures">Textures list.</param>
        /// <param name="images">Images list.</param>
        public static void Export(MaterialNode node, Document doc, ref IndexedDictionary<GLTFMaterial> materials, List<GLTFTexture> textures, List<GLTFImage> images)
        {
            ElementId id = node.MaterialId;
            GLTFMaterial gl_mat = new GLTFMaterial();
            float opacity = ONEINTVALUE - (float)node.Transparency;

            // Validate if the material is valid because for some reason there are
            // materials with invalid Ids
            if (id != ElementId.InvalidElementId)
            {
                string uniqueId;
                Material material = null;
                if (!MaterialNameContainer.TryGetValue(node.MaterialId, out var materialElement))
                {
                    // construct a material from the node
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

                GLTFPBR pbr = new GLTFPBR();
                SetMaterialsProperties(node, opacity, ref pbr, ref gl_mat);

                // Extract and set texture if available
                if (material != null)
                {
                    ExtractAndSetTexture(material, doc, ref gl_mat, textures, images);
                }

                materials.AddOrUpdateCurrentMaterial(uniqueId, gl_mat, false);
            }
        }

        private static void SetMaterialsProperties(MaterialNode node, float opacity, ref GLTFPBR pbr, ref GLTFMaterial gl_mat)
        {
            pbr.baseColorFactor = new List<float>(4) { node.Color.Red / 255f, node.Color.Green / 255f, node.Color.Blue / 255f, opacity };
            pbr.metallicFactor = 0f;
            pbr.roughnessFactor = opacity != 1 ? 0.5f : 1f;
            gl_mat.pbrMetallicRoughness = pbr;

            // TODO: Implement MASK alphamode for elements like leaves or wire fences
            gl_mat.alphaMode = opacity != 1 ? BLEND : OPAQUE;
            gl_mat.alphaCutoff = null;
        }

        private static void ExtractAndSetTexture(Material material, Document doc, ref GLTFMaterial gl_mat, List<GLTFTexture> textures, List<GLTFImage> images)
        {
            try
            {
                ElementId appearanceId = material.AppearanceAssetId;
                if (appearanceId == ElementId.InvalidElementId)
                    return;

                AppearanceAssetElement appearanceElem = doc.GetElement(appearanceId) as AppearanceAssetElement;
                if (appearanceElem == null)
                    return;

                Asset theAsset = appearanceElem.GetRenderingAsset();
                AssetProperty ap = theAsset.FindByName(GENERIC_DIFFUSE);

                string texturePath = null;

                if (ap != null && ap.NumberOfConnectedProperties > 0)
                {
                    var connectedAsset = ap.GetSingleConnectedAsset();
                    AssetPropertyString bitmapPathProp = connectedAsset.FindByName(UNIFIED_BITMAP) as AssetPropertyString;

                    if (bitmapPathProp != null)
                    {
                        texturePath = bitmapPathProp.Value.Split('|')[0];
                        if (!Path.IsPathRooted(texturePath))
                        {
                            texturePath = Path.Combine(Path.GetDirectoryName(doc.PathName), texturePath);
                        }
                    }
                }
                else
                {
                    int size = theAsset.Size;
                    if (size == 0) return;

                    for (int assetIdx = 0; assetIdx < size; assetIdx++)
                    {
                        var prop = theAsset.Get(assetIdx);
                        if (prop.NumberOfConnectedProperties > 0)
                        {
                            var connectedAsset = prop.GetSingleConnectedAsset();
                            AssetPropertyString bitmapPathProp = connectedAsset.FindByName("unifiedbitmap_Bitmap") as AssetPropertyString;

                            if (bitmapPathProp != null &&  prop.Name.ToLower().Contains("opaque_albedo"))
                            {
                                texturePath = bitmapPathProp.Value.Split('|')[0];
                                texturePath = texturePath.Replace("/", "\\");
                                string MaterialsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles), @"Autodesk Shared\Materials\Textures\");
                                if (!Path.IsPathRooted(texturePath))
                                {
                                    texturePath = Path.Combine(MaterialsPath, texturePath);
                                    string destinationRoot = @"C:\Users\User\Desktop";
                                    string texturesFolder = Path.Combine(destinationRoot, "textures");
                                    Directory.CreateDirectory(texturesFolder);

                                    string fileName = Path.GetFileName(texturePath);
                                    string destinationFilePath = Path.Combine(texturesFolder, fileName);

                                    File.Copy(texturePath, destinationFilePath);

                                    Uri rootUri = new Uri(destinationRoot + Path.DirectorySeparatorChar);
                                    Uri fileUri = new Uri(destinationFilePath);
                                    texturePath = Uri.UnescapeDataString(rootUri.MakeRelativeUri(fileUri).ToString());
                                }
                                break;
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(texturePath))
                {
                    AddTextureToMaterial(texturePath, ref gl_mat, textures, images);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting texture from material {material.Name}: {ex.Message}");
            }
        }

        private static void AddTextureToMaterial(string path, ref GLTFMaterial gl_mat, List<GLTFTexture> textures, List<GLTFImage> images)
        {
            // 1. Add image
            var image = new GLTFImage
            {
                uri = path,
                mimeType = GetMimeType(path)
            };
            images.Add(image);
            int imageIndex = images.Count - 1;

            // 2. Add texture referencing the image
            var texture = new GLTFTexture
            {
                source = imageIndex
            };
            textures.Add(texture);
            int textureIndex = textures.Count - 1;

            // 3. Add texture info to the material
            var textureInfo = new GLTFTextureInfo
            {
                index = textureIndex,
                texCoord = 0
            };

            gl_mat.pbrMetallicRoughness.baseColorTexture = textureInfo;
        }

        private static string GetMimeType(string path)
        {
            string extension = Path.GetExtension(path).ToLower();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".bmp":
                    return "image/bmp";
                case ".gif":
                    return "image/gif";
                case ".webp":
                    return "image/webp";
                default:
                    return "image/png"; // Default to PNG if unknown
            }
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
