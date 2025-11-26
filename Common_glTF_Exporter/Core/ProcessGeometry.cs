using Autodesk.Revit.DB;
using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Utils;
using Common_glTF_Exporter.Windows.MainWindow;
using glTF.Manipulator.GenericSchema;
using glTF.Manipulator.Schema;
using Revit_glTF_Exporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Common_glTF_Exporter.Core
{
    public static class ProcessGeometry
    {
        public static void ProcessNode(IndexedDictionary<Node> nodes, Node rootNode, IndexedDictionary<glTF.Manipulator.Schema.Mesh> meshes,
            Preferences preferences, GLTFBinaryData globalBuffer, List<BufferView> bufferViews, List<Accessor> accessors, IndexedDictionary<BaseMaterial> materials,
            IndexedDictionary<GeometryDataObject> dataObject, Element currentElement)
        {

            Extras extras = new Extras();

            if (preferences.properties)
            {
                var parameters = Util.GetElementParameters(currentElement, true);
                parameters["UniqueId"] = currentElement.UniqueId;

                if (currentElement.Category != null)
                    parameters["Category"] = currentElement.Category.Name;

                extras.parameters = parameters;
            }
           
            Node newNode = new Node()
            {
                name = Util.ElementDescription(currentElement),
                extras = extras
            };

            long elmId;

            #if REVIT2024 || REVIT2025 || REVIT2026
            elmId = currentElement.Id.Value;
            #else
            elmId = currentElement.Id.IntegerValue;
            #endif

            nodes.AddOrUpdateCurrent(currentElement.UniqueId, newNode);
            rootNode.children.Add(nodes.CurrentIndex);

            glTF.Manipulator.Schema.Mesh newMesh = new glTF.Manipulator.Schema.Mesh
            {
                name = currentElement.Name,
                primitives = new List<MeshPrimitive>()
            };

            meshes.AddOrUpdateCurrent(currentElement.UniqueId, newMesh);
            nodes.CurrentItem.mesh = meshes.CurrentIndex;

            foreach (KeyValuePair<string, GeometryDataObject> kvp in dataObject.Dict)
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
                    GLTFBinaryDataUtils.ExportBatchId(elmId, kvp.Value, gLTFBinaryDataElement, bufferViews, accessors, primitive);
                }

                int indexAcessor = GLTFBinaryDataUtils.ExportFaces(kvp.Value, gLTFBinaryDataElement, bufferViews, accessors);

                primitive.indices = indexAcessor;

                if (preferences.materials != MaterialsEnum.nonematerials)
                {
                    primitive.material = materials.GetIndexFromUUID(kvp.Value.MaterialInfo.uuid);
                }

                meshes.CurrentItem.primitives.Add(primitive);
                meshes.CurrentItem.name = currentElement.Name;

                var g = kvp.Value;
                g.Vertices.Clear();
                g.Normals.Clear();
                g.Uvs.Clear();
                g.Faces.Clear();
            }
        }
    }
}
