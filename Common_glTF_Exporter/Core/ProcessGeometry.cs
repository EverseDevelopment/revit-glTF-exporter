using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Utils;
using Common_glTF_Exporter.Windows.MainWindow;
using glTF.Manipulator.Schema;
using Revit_glTF_Exporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Common_glTF_Exporter.Core
{
    public static class ProcessGeometry
    {
        public static void ProcessNode(BaseNode node, IndexedDictionary<Node> nodes, Node rootNode, IndexedDictionary<glTF.Manipulator.Schema.Mesh> meshes,
            Preferences preferences, GLTFBinaryData globalBuffer, List<BufferView> bufferViews, List<Accessor> accessors, IndexedDictionary<BaseMaterial> materials)
        {
            Node newNode = new Node()
            {
                name = node.description,
                extras = node.extras
            };

            nodes.AddOrUpdateCurrent(node.uuid, newNode);
            rootNode.children.Add(nodes.CurrentIndex);

            glTF.Manipulator.Schema.Mesh newMesh = new glTF.Manipulator.Schema.Mesh
            {
                name = node.name,
                primitives = new List<MeshPrimitive>()
            };

            meshes.AddOrUpdateCurrent(node.uuid, newMesh);
            nodes.CurrentItem.mesh = meshes.CurrentIndex;

            foreach (var kvp in node.objects.Dict.ToList())
            {
                MeshPrimitive primitive = new MeshPrimitive();
                GLTFBinaryData gLTFBinaryDataElement = globalBuffer;

                if (preferences.materials == MaterialsEnum.textures)
                {
                    GLTFBinaryDataUtils.ExportUVs(kvp.Value, gLTFBinaryDataElement, bufferViews, accessors, primitive);
                }

                GLTFBinaryDataUtils.ExportVertices(kvp.Value, gLTFBinaryDataElement, bufferViews, accessors, primitive);

                if (preferences.normals)
                {
                    GLTFBinaryDataUtils.ExportNormals(kvp.Value, gLTFBinaryDataElement, bufferViews, accessors, primitive);
                }

                if (preferences.batchId)
                {
                    GLTFBinaryDataUtils.ExportBatchId(node.id, kvp.Value, gLTFBinaryDataElement, bufferViews, accessors, primitive);
                }

                int indexAcessor = GLTFBinaryDataUtils.ExportFaces(kvp.Value, gLTFBinaryDataElement, bufferViews, accessors);

                primitive.indices = indexAcessor;

                if (preferences.materials != MaterialsEnum.nonematerials)
                {
                    primitive.material = materials.GetIndexFromUUID(kvp.Value.MaterialInfo.uuid);
                }

                meshes.CurrentItem.primitives.Add(primitive);
                meshes.CurrentItem.name = node.name;
            }
        }
    }
}
