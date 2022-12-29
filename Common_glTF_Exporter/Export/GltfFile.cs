using Autodesk.Revit.DB;
using Common_glTF_Exporter.Windows.MainWindow;
using Newtonsoft.Json;
using Revit_glTF_Exporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Documents;

namespace Common_glTF_Exporter.Export
{
    public static class GltfFile
    {
        public static void Create(List<glTFScene> scenes, List<glTFNode> nodes, List<glTFMesh> meshes, List<glTFMaterial> materials,
            List<glTFBuffer> buffers, List<glTFBufferView> bufferViews, List<glTFAccessor> accessors, Preferences preferences) 
        {
            // Package the properties into a serializable container
            glTF model = new glTF();
            model.asset = new glTFVersion();
            model.scenes = scenes;
            model.nodes = nodes;
            model.meshes = meshes;

            if (materials.Any())
            {
                model.materials = materials;
            }

            model.buffers = buffers;
            model.bufferViews = bufferViews;
            model.accessors = accessors;

            // Write the *.gltf file
            string serializedModel = JsonConvert.SerializeObject(model,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            if (!preferences.batchId)
            {
                serializedModel = serializedModel.Replace(",\"_BATCHID\":0", "");
            }

            if (!preferences.normals)
            {
                serializedModel = serializedModel.Replace(",\"NORMAL\":0", "");
            }

            string gltfName = String.Concat(preferences.path, ".gltf");
            File.WriteAllText(gltfName, serializedModel);
        }
    }
}
