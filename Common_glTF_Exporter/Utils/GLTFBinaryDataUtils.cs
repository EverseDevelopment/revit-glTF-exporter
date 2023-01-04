namespace Common_glTF_Exporter.Utils
{
    using System;
    using System.Collections.Generic;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Model;
    using Revit_glTF_Exporter;

    public class GLTFBinaryDataUtils
    {
        public static int ExportFaces(int bufferIdx, int byteOffset, GeometryDataObject geomData, GLTFBinaryData bufferData, List<GLTFBufferView> bufferViews, List<GLTFAccessor> accessors)
        {
            foreach (var index in geomData.Faces)
            {
                bufferData.indexBuffer.Add(index);
            }

            // Get max and min for index data
            int[] faceMinMax = Util.GetScalarMinMax(bufferData.indexBuffer);

            // Add a faces / indexes buffer view
            int elementsPerIndex = 1;
            int bytesPerIndexElement = 4;
            int bytesPerIndex = elementsPerIndex * bytesPerIndexElement;
            int numIndexes = geomData.Faces.Count;
            int sizeOfIndexView = numIndexes * bytesPerIndex;
            GLTFBufferView facesView = new GLTFBufferView(bufferIdx, byteOffset, sizeOfIndexView, Targets.ELEMENT_ARRAY_BUFFER, string.Empty);
            bufferViews.Add(facesView);
            int facesViewIdx = bufferViews.Count - 1;

            // add a face accessor
            var count = geomData.Faces.Count / elementsPerIndex;
            var max = new List<float>() { faceMinMax[1] };
            var min = new List<float>() { faceMinMax[0] };
            GLTFAccessor faceAccessor = new GLTFAccessor(facesViewIdx, 0, ComponentType.UNSIGNED_INT, count, "SCALAR", max, min, "FACE");
            accessors.Add(faceAccessor);
            bufferData.indexAccessorIndex = accessors.Count - 1;
            return byteOffset + facesView.byteLength;
        }

        public static int ExportVertices(int bufferIdx, int byteOffset, GeometryDataObject geomData, GLTFBinaryData bufferData, List<GLTFBufferView> bufferViews, List<GLTFAccessor> accessors, out int sizeOfVec3View, out int elementsPerVertex)
        {
            foreach (var coord in geomData.Vertices)
            {
                float floatValue = Convert.ToSingle(coord);
                bufferData.vertexBuffer.Add(floatValue);
            }

            // Get max and min for vertex data
            float[] vertexMinMax = Util.GetVec3MinMax(bufferData.vertexBuffer);

            // Add a vec3 buffer view
            elementsPerVertex = 3;
            int bytesPerElement = 4;
            int bytesPerVertex = elementsPerVertex * bytesPerElement;
            int numVec3 = geomData.Vertices.Count / elementsPerVertex;
            sizeOfVec3View = numVec3 * bytesPerVertex;
            GLTFBufferView vec3View = new GLTFBufferView(bufferIdx, byteOffset, sizeOfVec3View, Targets.ARRAY_BUFFER, string.Empty);
            bufferViews.Add(vec3View);
            int vec3ViewIdx = bufferViews.Count - 1;

            // add a position accessor
            int count = geomData.Vertices.Count / elementsPerVertex;
            var max = new List<float>() { vertexMinMax[1], vertexMinMax[3], vertexMinMax[5] };
            var min = new List<float>() { vertexMinMax[0], vertexMinMax[2], vertexMinMax[4] };
            GLTFAccessor positionAccessor = new GLTFAccessor(vec3ViewIdx, 0, ComponentType.FLOAT, count, "VEC3", max, min, "POSITION");
            accessors.Add(positionAccessor);
            bufferData.vertexAccessorIndex = accessors.Count - 1;
            return byteOffset + vec3View.byteLength;
        }

        public static int ExportNormals(int bufferIdx, int byteOffset, GeometryDataObject geomData, GLTFBinaryData bufferData, List<GLTFBufferView> bufferViews, List<GLTFAccessor> accessors)
        {
            foreach (var normal in geomData.Normals)
            {
                float floatValue = Convert.ToSingle(normal);
                bufferData.normalBuffer.Add(floatValue);
            }

            // Get max and min for normal data
            float[] normalMinMax = Util.GetVec3MinMax(bufferData.normalBuffer);

            // Add a normals (vec3) buffer view
            int elementsPerNormal = 3;
            int bytesPerNormalElement = 4;
            int bytesPerNormal = elementsPerNormal * bytesPerNormalElement;
            int numVec3Normals = geomData.Normals.Count / elementsPerNormal;
            int sizeOfVec3ViewNormals = numVec3Normals * bytesPerNormal;
            GLTFBufferView vec3ViewNormals = new GLTFBufferView(bufferIdx, byteOffset, sizeOfVec3ViewNormals, Targets.ARRAY_BUFFER, string.Empty);
            bufferViews.Add(vec3ViewNormals);
            int vec3ViewNormalsIdx = bufferViews.Count - 1;

            // add a normals accessor
            var count = geomData.Normals.Count / elementsPerNormal;
            var max = new List<float>() { normalMinMax[1], normalMinMax[3], normalMinMax[5] };
            var min = new List<float>() { normalMinMax[0], normalMinMax[2], normalMinMax[4] };

            GLTFAccessor normalsAccessor = new GLTFAccessor(vec3ViewNormalsIdx, 0, ComponentType.FLOAT, count, "VEC3", max, min, "NORMALS");
            accessors.Add(normalsAccessor);
            bufferData.normalsAccessorIndex = accessors.Count - 1;
            return byteOffset + vec3ViewNormals.byteLength;
        }

        public static int ExportBatchId(int bufferIdx, int byteOffset, int sizeOfVec3View, int elementsPerVertex, int elementId, GeometryDataObject geomData, GLTFBinaryData bufferData, List<GLTFBufferView> bufferViews, List<GLTFAccessor> accessors)
        {
            foreach (var vertice in geomData.Vertices)
            {
                bufferData.batchIdBuffer.Add(elementId);
            }

            // Get max and min for batchId data
            float[] batchIdMinMax = Util.GetVec3MinMax(bufferData.batchIdBuffer);

            // Add a batchId buffer view
            GLTFBufferView batchIdsView = new GLTFBufferView(bufferIdx, byteOffset, sizeOfVec3View, Targets.ARRAY_BUFFER, string.Empty);
            bufferViews.Add(batchIdsView);
            int batchIdsViewIdx = bufferViews.Count - 1;

            // add a batchId accessor
            var count = geomData.Vertices.Count / elementsPerVertex;
            var max = new List<float>() { batchIdMinMax[1], batchIdMinMax[3], batchIdMinMax[5] };
            var min = new List<float>() { batchIdMinMax[0], batchIdMinMax[2], batchIdMinMax[4] };
            GLTFAccessor batchIdAccessor = new GLTFAccessor(batchIdsViewIdx, 0, ComponentType.FLOAT, count, "VEC3", max, min, "BATCH_ID");
            accessors.Add(batchIdAccessor);
            bufferData.batchIdAccessorIndex = accessors.Count - 1;
            return byteOffset + batchIdsView.byteLength;
        }
    }
}
