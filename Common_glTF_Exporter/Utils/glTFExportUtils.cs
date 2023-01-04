namespace Common_glTF_Exporter.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.UI;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Model;
    using Revit_glTF_Exporter;

    public class GLTFExportUtils
    {
        public static GLTFMaterial GetGLTFMaterial(List<GLTFMaterial> glTFMaterials, Material material, bool doubleSided)
        {
            // search for an already existing material
            var m = glTFMaterials.FirstOrDefault(x =>
            x.pbrMetallicRoughness.baseColorFactor[0] == material.Color.Red &&
            x.pbrMetallicRoughness.baseColorFactor[1] == material.Color.Green &&
            x.pbrMetallicRoughness.baseColorFactor[2] == material.Color.Blue && x.doubleSided == doubleSided);

            return m != null ? m : GLTFExportUtils.CreateGLTFMaterial("default", 0, new Color(250, 250, 250), doubleSided);
        }

        public static GLTFMaterial CreateGLTFMaterial(string materialName, int materialOpacity, Color color, bool doubleSided)
        {
            // construct the material
            GLTFMaterial gl_mat = new GLTFMaterial();
            gl_mat.doubleSided = doubleSided;
            float opacity = 1 - (float)materialOpacity;
            gl_mat.name = materialName;
            GLTFPBR pbr = new GLTFPBR();
            pbr.baseColorFactor = new List<float>() { color.Red / 255f, color.Green / 255f, color.Blue / 255f, opacity };
            pbr.metallicFactor = 0f;
            pbr.roughnessFactor = 1f;
            gl_mat.pbrMetallicRoughness = pbr;

            return gl_mat;
        }

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
            buffer.uri = string.Concat(name, ".bin");
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

        public static void AddNormals(bool flipCoordinates, Transform transform, PolymeshTopology polymesh, List<double> normals)
        {
            IList<XYZ> polymeshNormals = polymesh.GetNormals();

            switch (polymesh.DistributionOfNormals)
            {
                case DistributionOfNormals.AtEachPoint:
                {
                    foreach (PolymeshFacet facet in polymesh.GetFacets())
                    {
                        XYZ normal1 = transform.OfVector(polymeshNormals[facet.V1]);
                        XYZ normal2 = transform.OfVector(polymeshNormals[facet.V2]);
                        XYZ normal3 = transform.OfVector(polymeshNormals[facet.V3]);

                        var newNormal1 = normal1.FlipCoordinates();
                        var newNormal2 = normal2.FlipCoordinates();
                        var newNormal3 = normal3.FlipCoordinates();

                        normals.Add(newNormal1.X);
                        normals.Add(newNormal1.Y);
                        normals.Add(newNormal1.Z);
                        normals.Add(newNormal2.X);
                        normals.Add(newNormal2.Y);
                        normals.Add(newNormal2.Z);
                        normals.Add(newNormal3.X);
                        normals.Add(newNormal3.Y);
                        normals.Add(newNormal3.Z);
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

                            if (flipCoordinates)
                            {
                                newNormal = normal.FlipCoordinates();
                            }

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
                        newNormal = newNormal.FlipCoordinates();

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
