using System.Collections.Generic;
using System.IO;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Windows.MainWindow;
using Revit_glTF_Exporter;
using Common_glTF_Exporter.Materials;
using Common_glTF_Exporter.Model;


namespace Common_glTF_Exporter.Export
{
    public static class RevitMaterials
    {
        const int ONEINTVALUE = 1;
        const string REALWORLDSCALEX = "texture_RealWorldScaleX";
        const string REALWORLDSCALEY = "texture_RealWorldScaleY";
        const string GENERICDIFFUSEFADE = "generic_diffuse_image_fade";    

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
                MaterialProperties.SetProperties(node, opacity, ref pbr, ref gl_mat);

                // Instead of embedding the image now, just store the path for future export
                if (material != null && preferences.materials == MaterialsEnum.textures)
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
                        gl_mat.EmbeddedTexturePath = texturePath;

                        float scaleX = AssetPropertiesUtils.GetScale(connectedAsset, REALWORLDSCALEX);
                        float scaleY = AssetPropertiesUtils.GetScale(connectedAsset, REALWORLDSCALEY);

                        float rotation = AssetPropertiesUtils.GetRotationRadians(connectedAsset);

                    AssetPropertyDouble fadeProp = theAsset.FindByName(GENERICDIFFUSEFADE) as AssetPropertyDouble;

                        if (fadeProp != null)
                        {
                            gl_mat.Fadevalue = fadeProp.Value;
                            gl_mat.BaseColor = AssetPropertiesUtils.GetAppearenceColor(theAsset);
                            gl_mat.pbrMetallicRoughness.baseColorFactor = new List<float>(4)
                                {
                                    1,
                                    1,
                                    1,
                                    opacity
                                };
                        }

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

            return gl_mat;
        }
    }
}
