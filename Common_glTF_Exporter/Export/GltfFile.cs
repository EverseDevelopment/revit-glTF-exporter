namespace Common_glTF_Exporter.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Documents;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Windows.MainWindow;
    using Newtonsoft.Json;
    using Revit_glTF_Exporter;

    public static class GltfFile
    {
        public static void Create(
            List<GLTFScene> scenes,
            List<GLTFNode> nodes,
            List<GLTFMesh> meshes,
            List<GLTFMaterial> materials,
            List<GLTFBuffer> buffers,
            List<GLTFBufferView> bufferViews,
            List<GLTFAccessor> accessors,
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

            string gltfName = string.Concat(preferences.path, ".gltf");
            File.WriteAllText(gltfName, serializedModel);
        }
    }
}
