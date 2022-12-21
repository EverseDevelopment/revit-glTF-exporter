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
                //batchIdAccessor.max = new List<float>() { batchIdMinMax[1], batchIdMinMax[3], batchIdMinMax[5] };
                //batchIdAccessor.min = new List<float>() { batchIdMinMax[0], batchIdMinMax[2], batchIdMinMax[4] };
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
            //faceAccessor.count = numIndexes;
            faceAccessor.count = geomData.faces.Count / elementsPerIndex;
            faceAccessor.type = "SCALAR";
            //faceAccessor.max = new List<float>() { faceMinMax[1] };
            //faceAccessor.min = new List<float>() { faceMinMax[0] };
            faceAccessor.name = "FACE";
            accessors.Add(faceAccessor);
            bufferData.indexAccessorIndex = accessors.Count - 1;

            #endregion

            return bufferData;
        }
    }
}
