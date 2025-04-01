using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Windows.MainWindow;
using Newtonsoft.Json;
using Revit_glTF_Exporter;

namespace Common_glTF_Exporter.Export
{
    public static class GltfJson
    {
        public static string Get(
            List<GLTFScene> scenes,
            List<GLTFNode> nodes,
            List<GLTFMesh> meshes,
            List<GLTFMaterial> materials,
            List<GLTFBuffer> buffers,
            List<GLTFBufferView> bufferViews,
            List<GLTFAccessor> accessors,
            List<GLTFTexture> textures,
            List<GLTFImage> images,
            Preferences preferences)
        {
            // Package the properties into a serializable container
            GLTF model = new GLTF
            {
                asset = new GLTFVersion(),
                scenes = scenes,
                nodes = nodes,
                meshes = meshes,
            };

            if (materials.Any())
            {
                model.materials = materials;
            }

            if (textures.Any())
            {
                model.textures = textures;
            }

            if (images.Any())
            {
                model.images = images;
            }

            model.buffers = buffers;
            model.bufferViews = bufferViews;
            model.accessors = accessors;

            // Write the *.gltf file
            string serializedModel = JsonConvert.SerializeObject(
                model,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });


            if (!preferences.batchId)
            {
                serializedModel = serializedModel.Replace(",\"_BATCHID\":0", string.Empty);
            }

            if (!preferences.normals)
            {
                serializedModel = serializedModel.Replace(",\"NORMAL\":0", string.Empty);
            }

            return serializedModel;
        }
    }
}
