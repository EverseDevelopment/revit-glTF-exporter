namespace Common_glTF_Exporter.Utils
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Model;
    using Revit_glTF_Exporter;

    public class GLTFBinaryDataUtils
    {
        const string SCALAR_STR = "SCALAR";
        const string FACE_STR = "FACE";
        const string BATCH_ID_STR = "BATCH_ID";

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
            var max = new List<float>(1) { faceMinMax[1] };
            var min = new List<float>(1) { faceMinMax[0] };
            GLTFAccessor faceAccessor = new GLTFAccessor(facesViewIdx, 0, ComponentType.UNSIGNED_INT, count, SCALAR_STR, max, min, FACE_STR);
            accessors.Add(faceAccessor);
            bufferData.indexAccessorIndex = accessors.Count - 1;
            return byteOffset + facesView.byteLength;
        }

        const string VEC3_STR = "VEC3";
        const string POSITION_STR = "POSITION";

        public static int ExportVertices(int bufferIdx, int byteOffset, GeometryDataObject geomData, GLTFBinaryData bufferData, List<GLTFBufferView> bufferViews, List<GLTFAccessor> accessors, out int sizeOfVec3View, out int elementsPerVertex)
        {

            for (int i = 0; i < geomData.Vertices.Count; i++)
            {
                bufferData.vertexBuffer.Add(Convert.ToSingle(geomData.Vertices[i]));
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
            var max = new List<float>(3) { vertexMinMax[1], vertexMinMax[3], vertexMinMax[5] };
            var min = new List<float>(3) { vertexMinMax[0], vertexMinMax[2], vertexMinMax[4] };

            GLTFAccessor positionAccessor = new GLTFAccessor(vec3ViewIdx, 0, ComponentType.FLOAT, count, VEC3_STR, max, min, POSITION_STR);
            accessors.Add(positionAccessor);
            bufferData.vertexAccessorIndex = accessors.Count - 1;
            return byteOffset + vec3View.byteLength;
        }

        const string NORMAL_STR = "NORMALS";

        public static int ExportNormals(int bufferIdx, int byteOffset, GeometryDataObject geomData, GLTFBinaryData bufferData, List<GLTFBufferView> bufferViews, List<GLTFAccessor> accessors)
        {
            for (int i = 0; i < geomData.Normals.Count; i++)
            {
                bufferData.normalBuffer.Add(Convert.ToSingle(geomData.Normals[i]));
            }

            // Get max and min for normal data
            float[] normalMinMax = Util.GetVec3MinMax(bufferData.normalBuffer);

            // Add a normals (vec3) buffer view
            int elementsPerNormal = 3;
            int bytesPerNormalElement = 4;
            int bytesPerNormal = elementsPerNormal * bytesPerNormalElement;
            var normalsCount = geomData.Normals.Count;
            int numVec3Normals = normalsCount / elementsPerNormal;
            int sizeOfVec3ViewNormals = numVec3Normals * bytesPerNormal;
            GLTFBufferView vec3ViewNormals = new GLTFBufferView(bufferIdx, byteOffset, sizeOfVec3ViewNormals, Targets.ARRAY_BUFFER, string.Empty);
            bufferViews.Add(vec3ViewNormals);
            int vec3ViewNormalsIdx = bufferViews.Count - 1;

            // add a normals accessor
            var count = normalsCount / elementsPerNormal;
            var max = new List<float>(3) { normalMinMax[1], normalMinMax[3], normalMinMax[5] };
            var min = new List<float>(3) { normalMinMax[0], normalMinMax[2], normalMinMax[4] };

            GLTFAccessor normalsAccessor = new GLTFAccessor(vec3ViewNormalsIdx, 0, ComponentType.FLOAT, count, VEC3_STR, max, min, NORMAL_STR);
            accessors.Add(normalsAccessor);
            bufferData.normalsAccessorIndex = accessors.Count - 1;
            return byteOffset + vec3ViewNormals.byteLength;
        }

        public static int ExportBatchId(int bufferIdx, int byteOffset, int sizeOfVec3View, int elementsPerVertex, long elementId, GeometryDataObject geomData, GLTFBinaryData bufferData, List<GLTFBufferView> bufferViews, List<GLTFAccessor> accessors)
        {
            for (int i = 0; i < geomData.Vertices.Count; i++)
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
            var max = new List<float>(3) { batchIdMinMax[1], batchIdMinMax[3], batchIdMinMax[5] };
            var min = new List<float>(3) { batchIdMinMax[0], batchIdMinMax[2], batchIdMinMax[4] };
            GLTFAccessor batchIdAccessor = new GLTFAccessor(batchIdsViewIdx, 0, ComponentType.FLOAT, count, VEC3_STR, max, min, BATCH_ID_STR);
            accessors.Add(batchIdAccessor);
            bufferData.batchIdAccessorIndex = accessors.Count - 1;
            return byteOffset + batchIdsView.byteLength;
        }

        public static int ExportUVs(
               int bufferIdx,
               int byteOffset,
               GeometryDataObject geomData,
               GLTFBinaryData bufferData,
               List<GLTFBufferView> bufferViews,
               List<GLTFAccessor> accessors)
        {
            const string VEC2_STR = "VEC2";
            const string TEXCOORD_STR = "TEXCOORD_0";

            int uvCount = geomData.Uvs.Count;

            // Convert UVs to float buffer (U, V per entry)
            foreach (var uv in geomData.Uvs)
            {
                bufferData.uvBuffer.Add((float)uv.U);
                bufferData.uvBuffer.Add((float)uv.V);
            }

            int elementsPerUV = 2;
            int bytesPerUVElement = 4;
            int bytesPerUV = elementsPerUV * bytesPerUVElement;


            int sizeOfUVView = uvCount * bytesPerUV;

            // Create UV buffer view
            GLTFBufferView uvBufferView = new GLTFBufferView(bufferIdx, byteOffset, sizeOfUVView, Targets.ARRAY_BUFFER, string.Empty);
            bufferViews.Add(uvBufferView);
            int uvBufferViewIdx = bufferViews.Count - 1;

            // Min/max for VEC2 accessors (optional, but good practice)
            float[] uvMinMax = Util.GetVec2MinMax(bufferData.uvBuffer);
            var max = new List<float> { uvMinMax[1], uvMinMax[3] };
            var min = new List<float> { uvMinMax[0], uvMinMax[2] };

            // Create UV accessor
            GLTFAccessor uvAccessor = new GLTFAccessor(
                uvBufferViewIdx,
                0,
                ComponentType.FLOAT,
                uvCount,
                VEC2_STR,
                max,
                min,
                TEXCOORD_STR);

            accessors.Add(uvAccessor);
            bufferData.uvAccessorIndex = accessors.Count - 1;

            return byteOffset + uvBufferView.byteLength;
        }

        public static int ExportImageBuffer(
            int bufferIdx,
            int byteOffset,
            GLTFMaterial material,
            List<GLTFImage> images,
            List<GLTFTexture> textures,
            GLTFBinaryData bufferData,
            List<GLTFBufferView> bufferViews)
        {
            if (material.EmbeddedTexturePath == null)
            {
                return byteOffset;
            }

            if (material.pbrMetallicRoughness.baseColorTexture.index == -1)
            {
                byte[] imageBytes = File.ReadAllBytes(material.EmbeddedTexturePath);
                string mimeType = GetMimeType(material.EmbeddedTexturePath);

                if (imageBytes != null)
                {
                    if (bufferData.byteData == null)
                    {
                        bufferData.byteData = imageBytes;
                    }
                    else
                    {
                        byte[] combined = new byte[bufferData.byteData.Length + imageBytes.Length];
                        Buffer.BlockCopy(bufferData.byteData, 0, combined, 0, bufferData.byteData.Length);
                        Buffer.BlockCopy(imageBytes, 0, combined, bufferData.byteData.Length, imageBytes.Length);
                        bufferData.byteData = combined;
                    }
                }

                int currentLenght = imageBytes.Length;
                int alignment = 4;
                int padding = (alignment - (currentLenght % alignment)) % alignment;

                if (padding != 0)
                {
                    currentLenght = currentLenght + padding;

                    byte[] newArray = bufferData.byteData.Concat(new byte[padding]).ToArray();
                    bufferData.byteData = newArray;
                }

                GLTFBufferView ImageBufferView = new GLTFBufferView(bufferIdx, byteOffset, currentLenght, Targets.NONE, string.Empty);

                bufferViews.Add(ImageBufferView);
                int bufferViewIndex = bufferViews.Count - 1;

                var image = new GLTFImage
                {
                    bufferView = bufferViewIndex,
                    mimeType = mimeType
                };

                images.Add(image);
                int imageIndex = images.Count - 1;

                var texture = new GLTFTexture
                {
                    source = imageIndex
                };
                textures.Add(texture);
                int textureIndex = textures.Count - 1;
                material.pbrMetallicRoughness.baseColorTexture.index = textureIndex;
                return byteOffset + ImageBufferView.byteLength;
            }
            else
            {
                return byteOffset;
            }
        }




        private static string GetMimeType(string path)
        {
            string extension = System.IO.Path.GetExtension(path).ToLower();
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                case ".bmp":
                    return "image/bmp";
                case ".gif":
                    return "image/gif";
                case ".webp":
                    return "image/webp";
                default:
                    return "image/png"; // Default to PNG if unknown
            }
        }
    }
}
