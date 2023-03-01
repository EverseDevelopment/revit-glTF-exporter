namespace Common_glTF_Exporter.Utils
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Model;
    using Common_glTF_Exporter.Windows.MainWindow;
    using Revit_glTF_Exporter;

    public class GLTFExportUtils
    {
        const int DEF_COLOR = 250;
        const string DEF_MATERIAL_NAME = "default";

        public static GLTFMaterial GetGLTFMaterial(List<GLTFMaterial> gltfMaterials, Material material, bool doubleSided)
        {
            // search for an already existing material
            var m = gltfMaterials.FirstOrDefault(x =>
            x.pbrMetallicRoughness.baseColorFactor[0] == material.Color.Red &&
            x.pbrMetallicRoughness.baseColorFactor[1] == material.Color.Green &&
            x.pbrMetallicRoughness.baseColorFactor[2] == material.Color.Blue && x.doubleSided == doubleSided);

            return m != null ? m : GLTFExportUtils.CreateGLTFMaterial(DEF_MATERIAL_NAME, 0, new Color(DEF_COLOR, DEF_COLOR, DEF_COLOR), doubleSided);
        }

        public static GLTFMaterial CreateGLTFMaterial(string materialName, int materialOpacity, Color color, bool doubleSided)
        {
            // construct the material
            GLTFMaterial gl_mat = new GLTFMaterial();
            gl_mat.doubleSided = doubleSided;
            float opacity = 1 - (float)materialOpacity;
            gl_mat.name = materialName;
            GLTFPBR pbr = new GLTFPBR();
            pbr.baseColorFactor = new List<float>(4) { color.Red / 255f, color.Green / 255f, color.Blue / 255f, opacity };
            pbr.metallicFactor = 0f;
            pbr.roughnessFactor = 1f;
            gl_mat.pbrMetallicRoughness = pbr;

            return gl_mat;
        }

        public static void AddVerticesAndFaces(VertexLookupIntObject vertex, GeometryDataObject geometryDataObject, MeshTriangle triangle)
        {
            var idx = vertex.AddVertex(new PointIntObject(triangle.get_Vertex(0)));
            geometryDataObject.Faces.Add(idx);

            var idx1 = vertex.AddVertex(new PointIntObject(triangle.get_Vertex(1)));
            geometryDataObject.Faces.Add(idx1);

            var idx2 = vertex.AddVertex(new PointIntObject(triangle.get_Vertex(2)));
            geometryDataObject.Faces.Add(idx2);
        }

        const string UNDERSCORE = "_";

        public static void AddOrUpdateCurrentItem(
            IndexedDictionary<GLTFNode> nodes,
            IndexedDictionary<GeometryDataObject> geomDataObj,
            IndexedDictionary<VertexLookupIntObject> vertexIntObj,
            IndexedDictionary<GLTFMaterial> materials)
        {
            // Add new "_current" entries if vertex_key is unique
            string vertex_key = string.Concat(nodes.CurrentKey, UNDERSCORE, materials.CurrentKey);
            geomDataObj.AddOrUpdateCurrent(vertex_key, new GeometryDataObject());
            vertexIntObj.AddOrUpdateCurrent(vertex_key, new VertexLookupIntObject());
        }

        public static void AddRPCNormals(Preferences preferences, MeshTriangle triangle, GeometryDataObject geomDataObj)
        {
            XYZ normal = GeometryUtils.GetNormal(triangle);

            for (int j = 0; j < 3; j++)
            {
                geomDataObj.Normals.Add(normal.X);
                geomDataObj.Normals.Add(normal.Y);
                geomDataObj.Normals.Add(normal.Z);
            }
        }

        const string BIN = ".bin";

        /// <summary>
        /// Takes the intermediate geometry data and performs the calculations
        /// to convert that into glTF buffers, views, and accessors.
        /// </summary>
        /// <param name="buffers">buffers.</param>
        /// <param name="accessors">accessors.</param>
        /// <param name="bufferViews">bufferViews.</param>
        /// <param name="geomData">geomData.</param>
        /// <param name="name">Unique name for the .bin file that will be produced.</param>
        /// <param name="elementId">Revit element's Element ID that will be used as the batchId value.</param>
        /// <param name="exportBatchId">exportBatchId.</param>
        /// <param name="exportNormals">exportNormals.</param>
        /// <returns>Returns the GLTFBinaryData object.</returns>
        public static GLTFBinaryData AddGeometryMeta(List<GLTFBuffer> buffers, List<GLTFAccessor> accessors, List<GLTFBufferView> bufferViews, GeometryDataObject geomData, string name, int elementId, bool exportBatchId, bool exportNormals)
        {
            int byteOffset = 0;

            // add a buffer
            GLTFBuffer buffer = new GLTFBuffer();
            buffer.uri = string.Concat(name, BIN);
            buffers.Add(buffer);
            int bufferIdx = buffers.Count - 1;
            GLTFBinaryData bufferData = new GLTFBinaryData();
            bufferData.name = buffer.uri;

            byteOffset = GLTFBinaryDataUtils.ExportVertices(bufferIdx, byteOffset, geomData, bufferData, bufferViews, accessors, out int sizeOfVec3View, out int elementsPerVertex);

            if (exportNormals)
            {
                byteOffset = GLTFBinaryDataUtils.ExportNormals(bufferIdx, byteOffset, geomData, bufferData, bufferViews, accessors);
            }

            if (exportBatchId)
            {
                byteOffset = GLTFBinaryDataUtils.ExportBatchId(bufferIdx, byteOffset, sizeOfVec3View, elementsPerVertex, elementId, geomData, bufferData, bufferViews, accessors);
            }

            byteOffset = GLTFBinaryDataUtils.ExportFaces(bufferIdx, byteOffset, geomData, bufferData, bufferViews, accessors);

            return bufferData;
        }

        public static void AddNormals(Preferences preferences, Transform transform, PolymeshTopology polymesh, List<double> normals)
        {
            IList<XYZ> polymeshNormals = polymesh.GetNormals();

            switch (polymesh.DistributionOfNormals)
            {
                case DistributionOfNormals.AtEachPoint:
                {
                    foreach (PolymeshFacet facet in polymesh.GetFacets())
                    {
                        List<XYZ> normalPoints = new List<XYZ>
                        {
                            transform.OfVector(polymeshNormals[facet.V1]),
                            transform.OfVector(polymeshNormals[facet.V2]),
                            transform.OfVector(polymeshNormals[facet.V3]),
                        };

                        foreach (var normalPoint in normalPoints)
                        {
                            XYZ newNormalPoint = normalPoint;

                            normals.Add(newNormalPoint.X);
                            normals.Add(newNormalPoint.Y);
                            normals.Add(newNormalPoint.Z);
                        }
                    }

                    break;
                }

                case DistributionOfNormals.OnePerFace:
                {
                    foreach (var facet in polymesh.GetFacets())
                    {
                        foreach (var normal in polymesh.GetNormals())
                        {
                            var newNormal = normal;

                            for (int j = 0; j < 3; j++)
                            {
                                normals.Add(newNormal.X);
                                normals.Add(newNormal.Y);
                                normals.Add(newNormal.Z);
                            }
                        }
                    }

                    break;
                }

                case DistributionOfNormals.OnEachFacet:
                {
                    foreach (XYZ normal in polymeshNormals)
                    {
                        var newNormal = transform.OfVector(normal);

                        normals.Add(newNormal.X);
                        normals.Add(newNormal.Y);
                        normals.Add(newNormal.Z);
                    }

                    break;
                }
            }
        }
    }
}
