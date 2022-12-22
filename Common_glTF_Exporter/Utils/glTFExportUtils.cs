using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Revit_glTF_Exporter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Utils
{
    public class glTFExportUtils
    {
        /// <summary>
        /// Takes the intermediate geometry data and performs the calculations
        /// to convert that into glTF buffers, views, and accessors.
        /// </summary>
        /// <param name="geomData"></param>
        /// <param name="name">Unique name for the .bin file that will be produced.</param>
        /// <param name="elementId">Revit element's Element ID that will be used as the batchId value.</param>
        /// <returns></returns>
        public static glTFBinaryData AddGeometryMeta(List<glTFBuffer> buffers, List<glTFAccessor> accessors, List<glTFBufferView> bufferViews, GeometryData geomData, string name, int elementId, bool exportBatchId, bool exportNormals)
        {
            // add a buffer
            glTFBuffer buffer = new glTFBuffer();
            buffer.uri = String.Concat(name, ".bin");
            buffers.Add(buffer);
            int bufferIdx = buffers.Count - 1;

            #region Buffer Data

            glTFBinaryData bufferData = new glTFBinaryData();
            bufferData.name = buffer.uri;

            foreach (var coord in geomData.vertices)
            {
                float vFloat = Convert.ToSingle(coord);
                bufferData.vertexBuffer.Add(vFloat);
            }

            foreach (var index in geomData.faces)
            {
                bufferData.indexBuffer.Add(index);
            }

            if (exportBatchId)
            {
                foreach (var vertice in geomData.vertices)
                {
                    bufferData.batchIdBuffer.Add(elementId);
                }
            }

            if (exportNormals)
            {
                foreach (var normal in geomData.normals)
                {
                    float vFloat = Convert.ToSingle(normal);
                    bufferData.normalBuffer.Add(vFloat);
                }
            }

            #endregion

            #region Max and Min

            // Get max and min for vertex data
            float[] vertexMinMax = Util.GetVec3MinMax(bufferData.vertexBuffer);

            // Get max and min for index data
            int[] faceMinMax = Util.GetScalarMinMax(bufferData.indexBuffer);

            // Get max and min for batchId data
            float[] batchIdMinMax = default;
            if (exportBatchId)
            {
                batchIdMinMax = Util.GetVec3MinMax(bufferData.batchIdBuffer);
            }

            //Get max and min for normal data
            float[] normalMinMax = default;
            if (exportNormals)
            {
                normalMinMax = Util.GetVec3MinMax(bufferData.normalBuffer);
            }

            #endregion

            // Buffer views and accessors

            #region Position 

            //Add a vec3 buffer view
            int elementsPerVertex = 3;
            int bytesPerElement = 4;
            int bytesPerVertex = elementsPerVertex * bytesPerElement;
            int numVec3 = (geomData.vertices.Count) / elementsPerVertex;
            int sizeOfVec3View = numVec3 * bytesPerVertex;
            glTFBufferView vec3View = new glTFBufferView();
            var byteOffset = 0;
            vec3View.buffer = bufferIdx;
            vec3View.byteOffset = byteOffset;
            vec3View.byteLength = sizeOfVec3View;
            vec3View.target = Targets.ARRAY_BUFFER;
            bufferViews.Add(vec3View);
            int vec3ViewIdx = bufferViews.Count - 1;

            // add a position accessor
            glTFAccessor positionAccessor = new glTFAccessor();
            positionAccessor.bufferView = vec3ViewIdx;
            positionAccessor.byteOffset = 0;
            positionAccessor.componentType = ComponentType.FLOAT;
            positionAccessor.count = geomData.vertices.Count / elementsPerVertex;
            positionAccessor.type = "VEC3";
            positionAccessor.max = new List<float>() { vertexMinMax[1], vertexMinMax[3], vertexMinMax[5] };
            positionAccessor.min = new List<float>() { vertexMinMax[0], vertexMinMax[2], vertexMinMax[4] };
            positionAccessor.name = "POSITION";
            accessors.Add(positionAccessor);
            bufferData.vertexAccessorIndex = accessors.Count - 1;
            byteOffset += vec3View.byteLength;

            #endregion

            #region Normals 

            if (exportNormals)
            {
                // Add a normals (vec3) buffer view
                int elementsPerNormal = 3;
                int bytesPerNormalElement = 4;
                int bytesPerNormal = elementsPerNormal * bytesPerNormalElement;
                int numVec3Normals = (geomData.normals.Count) / elementsPerNormal;
                int sizeOfVec3ViewNormals = numVec3Normals * bytesPerNormal;
                glTFBufferView vec3ViewNormals = new glTFBufferView();
                vec3ViewNormals.buffer = bufferIdx;
                vec3ViewNormals.byteOffset = byteOffset;
                vec3ViewNormals.byteLength = sizeOfVec3ViewNormals;
                vec3ViewNormals.target = Targets.ARRAY_BUFFER;
                bufferViews.Add(vec3ViewNormals);
                int vec3ViewNormalsIdx = bufferViews.Count - 1;

                //add a normals accessor
                glTFAccessor normalsAccessor = new glTFAccessor();
                normalsAccessor.bufferView = vec3ViewNormalsIdx;
                normalsAccessor.byteOffset = 0;
                normalsAccessor.componentType = ComponentType.FLOAT;
                normalsAccessor.count = geomData.normals.Count / elementsPerNormal;
                normalsAccessor.type = "VEC3";
                normalsAccessor.max = new List<float>() { normalMinMax[1], normalMinMax[3], normalMinMax[5] };
                normalsAccessor.min = new List<float>() { normalMinMax[0], normalMinMax[2], normalMinMax[4] };
                normalsAccessor.name = "NORMALS";
                accessors.Add(normalsAccessor);
                bufferData.normalsAccessorIndex = accessors.Count - 1;
                byteOffset += vec3ViewNormals.byteLength;
            }

            #endregion

            #region BatchId

            if (exportBatchId)
            {
                // Add a batchId buffer view
                glTFBufferView batchIdsView = new glTFBufferView();
                batchIdsView.buffer = bufferIdx;
                batchIdsView.byteOffset = byteOffset;
                batchIdsView.byteLength = sizeOfVec3View;
                batchIdsView.target = Targets.ARRAY_BUFFER;
                bufferViews.Add(batchIdsView);
                int batchIdsViewIdx = bufferViews.Count - 1;

                // add a batchId accessor
                glTFAccessor batchIdAccessor = new glTFAccessor();
                batchIdAccessor.bufferView = batchIdsViewIdx;
                batchIdAccessor.byteOffset = 0;
                batchIdAccessor.componentType = ComponentType.FLOAT;
                batchIdAccessor.count = geomData.vertices.Count / elementsPerVertex;
                batchIdAccessor.type = "VEC3";
                batchIdAccessor.max = new List<float>() { batchIdMinMax[1], batchIdMinMax[3], batchIdMinMax[5] };
                batchIdAccessor.min = new List<float>() { batchIdMinMax[0], batchIdMinMax[2], batchIdMinMax[4] };
                batchIdAccessor.name = "BATCH_ID";
                accessors.Add(batchIdAccessor);
                bufferData.batchIdAccessorIndex = accessors.Count - 1;
                byteOffset += batchIdsView.byteLength;
            }

            #endregion

            #region Faces

            // Add a faces / indexes buffer view
            int elementsPerIndex = 1;
            int bytesPerIndexElement = 4;
            int bytesPerIndex = elementsPerIndex * bytesPerIndexElement;
            int numIndexes = geomData.faces.Count;
            int sizeOfIndexView = numIndexes * bytesPerIndex;
            glTFBufferView facesView = new glTFBufferView();
            facesView.buffer = bufferIdx;
            facesView.byteOffset = byteOffset;
            facesView.byteLength = sizeOfIndexView;
            facesView.target = Targets.ELEMENT_ARRAY_BUFFER;
            bufferViews.Add(facesView);
            int facesViewIdx = bufferViews.Count - 1;

            // add a face accessor
            glTFAccessor faceAccessor = new glTFAccessor();
            faceAccessor.bufferView = facesViewIdx;
            faceAccessor.byteOffset = 0;
            faceAccessor.componentType = ComponentType.UNSIGNED_INT;
            faceAccessor.count = geomData.faces.Count / elementsPerIndex;
            faceAccessor.type = "SCALAR";
            faceAccessor.max = new List<float>() { faceMinMax[1] };
            faceAccessor.min = new List<float>() { faceMinMax[0] };
            faceAccessor.name = "FACE";
            accessors.Add(faceAccessor);
            bufferData.indexAccessorIndex = accessors.Count - 1;

            #endregion

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
