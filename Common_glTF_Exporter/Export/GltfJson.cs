using Autodesk.Revit.DB;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Utils;
using Common_glTF_Exporter.Windows.MainWindow;
using glTF.Manipulator.Schema;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Buffer = glTF.Manipulator.Schema.Buffer;

namespace Common_glTF_Exporter.Export
{
    public static class GltfJson
    {
        public static string Get(
            List<Scene> scenes,
            List<Node> nodes,
            List<glTF.Manipulator.Schema.Mesh> meshes,
            List<BaseMaterial> materials,
            List<Buffer> buffers,
            List<BufferView> bufferViews,
            List<Accessor> accessors,
            List<BaseTexture> textures,
            List<BaseImage> images,
            Preferences preferences)
        {

            glTF.Manipulator.Schema.Version version = new glTF.Manipulator.Schema.Version
            {
                version = "2.0",
                generator = string.Concat("e-verse custom generator ", SettingsConfig.currentVersion),
                copyright = "free tool created by e-verse"
            };

            GLTF model = new GLTF
            {
                asset = version,
                scenes = scenes,
                nodes = nodes,
                meshes = meshes,
            };

            if (preferences.materials == MaterialsEnum.textures)
            {
                model.extensionsUsed = new List<string> { "KHR_texture_transform" };
            }

            if (materials.Any() && preferences.materials != MaterialsEnum.nonematerials)
            {
                model.materials = transformMaterials(materials);
            }

            if (preferences.materials == MaterialsEnum.textures)
            {
                if (textures.Any())
                {
                    model.textures = transformTextures(textures);
                }

                if (images.Any())
                {
                    model.images = transformImages(images);
                }
            }

            model.buffers = buffers;
            model.bufferViews = bufferViews;
            model.accessors = accessors;

            var options = new JsonSerializerOptions
            {

                PropertyNamingPolicy = null,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false
            };

            string serializedModel = System.Text.Json.JsonSerializer.Serialize(model, options);

            return serializedModel;
        }

        public static List<Image> transformImages(List<BaseImage> baseImages)
        { 
            List<Image> images = new List<Image>();
            foreach (BaseImage baseImg in baseImages)
            { 
                Image img = new Image();
                img.bufferView = baseImg.bufferView;
                img.mimeType = baseImg.mimeType;
                images.Add(img);
            }
            return images;
        }

        public static List<Texture> transformTextures(List<BaseTexture> baseTextures)
        {
            List<Texture> textures = new List<Texture>();
            foreach (BaseTexture baseTxt in baseTextures)
            {
                Texture txt = new Texture();
                txt.source = baseTxt.imageIndex;
                textures.Add(txt);
            }
            return textures;
        }

        public static List<glTF.Manipulator.Schema.Material> transformMaterials(List<BaseMaterial> baseMaterials)
        {
            List<glTF.Manipulator.Schema.Material> materials = new List<glTF.Manipulator.Schema.Material>();
            foreach (BaseMaterial baseMat in baseMaterials)
            {
                glTF.Manipulator.Schema.Material mat = new glTF.Manipulator.Schema.Material();
                mat.alphaMode = baseMat.alphaMode;
                mat.alphaCutoff = baseMat.alphaCutoff;
                mat.name = baseMat.name;
                mat.doubleSided = baseMat.doubleSided;


                    PBR pbrMetallicRoughness = new PBR();
                    pbrMetallicRoughness.metallicFactor = baseMat.metallicFactor;
                    pbrMetallicRoughness.baseColorFactor = baseMat.baseColorFactor;
                    pbrMetallicRoughness.roughnessFactor = baseMat.roughnessFactor;
                    
                    if (baseMat.hasTexture)
                    {
                        TextureInfo baseColorTexture = new TextureInfo();
                        baseColorTexture.index = baseMat.textureIndex;

                        
                        KHR_texture_transform kHR_Texture_Transform = new KHR_texture_transform();
                        kHR_Texture_Transform.rotation = baseMat.rotation;
                        kHR_Texture_Transform.scale = baseMat.scale;
                        kHR_Texture_Transform.offset = baseMat.offset;

                        TextureExtensions extensions = new TextureExtensions();
                        extensions.KHR_texture_transform = kHR_Texture_Transform;
                        
                        baseColorTexture.extensions = extensions;
                        pbrMetallicRoughness.baseColorTexture = baseColorTexture;
                    }

                    mat.pbrMetallicRoughness = pbrMetallicRoughness;

                materials.Add(mat);
            }
            return materials;
        }
    }
}
