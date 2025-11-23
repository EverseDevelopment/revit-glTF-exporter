using Autodesk.Revit.DB;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Materials;
using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Windows.MainWindow;
using glTF.Manipulator.Schema;
using Revit_glTF_Exporter;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Common_glTF_Exporter.Utils
{
    public class GLTFBinaryDataUtils
    {
        const string SCALAR_STR = "SCALAR";
        const string FACE_STR = "FACE";
        const string BATCH_ID_STR = "BATCH_ID";

        public static int ExportFaces(
            GeometryDataObject geomData,
            GLTFBinaryData bufferData,
            List<BufferView> bufferViews,
            List<Accessor> accessors)
        {
            const string SCALAR_STR = "SCALAR";
            const string FACE_STR = "FACE";

            int indexCount = geomData.Faces.Count;
            if (indexCount == 0)
                return -1;

            int[] indices = geomData.Faces.ToArray();

            List<int> listIndicess = new List<int>(indices);
            int[] minMax = Util.GetScalarMinMax(listIndicess);

            var max = new List<float> { minMax[1] };
            var min = new List<float> { minMax[0] };

            int byteOffset = bufferData.AppendIntArray(indices);
            int byteLength = indices.Length * sizeof(int);

            BufferView view = new BufferView(
                buffer: 0,
                byteOffset: byteOffset,
                byteLength: byteLength,
                Targets.ELEMENT_ARRAY_BUFFER,
                name: ""
            );

            bufferViews.Add(view);
            int viewIdx = bufferViews.Count - 1;

            Accessor accessor = new Accessor(
                bufferView: viewIdx,
                byteOffset: 0,
                componentType: ComponentType.UNSIGNED_INT, // 5125
                count: indexCount,
                type: SCALAR_STR,
                max: max,
                min: min,
                name: FACE_STR
            );

            accessors.Add(accessor);
            int accessorIdx = accessors.Count - 1;

            return accessorIdx;
        }


        const string VEC3_STR = "VEC3";
        const string POSITION_STR = "POSITION";

        public static int ExportVertices(
            GeometryDataObject geomData,
            GLTFBinaryData bufferData,
            List<BufferView> bufferViews,
            List<Accessor> accessors,
            MeshPrimitive primitive)
        {
            const string VEC3_STR = "VEC3";
            const string POSITION_STR = "POSITION";

            int vertexCount = geomData.Vertices.Count / 3;
            if (vertexCount == 0)
                return -1;

            // Convert to float array
            float[] vertexFloats = new float[geomData.Vertices.Count];
            for (int i = 0; i < geomData.Vertices.Count; i++)
                vertexFloats[i] = (float)geomData.Vertices[i];

            // Compute min/max 
            float[] minMax = Util.GetVec3MinMax(vertexFloats);
            var max = new List<float> { minMax[1], minMax[3], minMax[5] };
            var min = new List<float> { minMax[0], minMax[2], minMax[4] };

            // Write to buffer (correct padding applied)
            int byteOffset = bufferData.AppendFloatArray(vertexFloats);
            int byteLength = vertexFloats.Length * sizeof(float);

            // Create bufferView
            var view = new BufferView(
                buffer: 0,
                byteOffset: byteOffset,
                byteLength: byteLength,
                Targets.ARRAY_BUFFER,
                name: ""
            );

            bufferViews.Add(view);
            int bufferViewIndex = bufferViews.Count - 1;

            // Create accessor
            var accessor = new Accessor(
                bufferView: bufferViewIndex,
                byteOffset: 0,
                componentType: ComponentType.FLOAT,
                count: vertexCount,
                type: VEC3_STR,
                max: max,
                min: min,
                name: POSITION_STR
            );

            accessors.Add(accessor);
            int accessorIndex = accessors.Count - 1;

            // Assign accessor to primitive
            primitive.attributes.POSITION = accessorIndex;

            return accessorIndex;
        }



        const string NORMAL_STR = "NORMALS";

        public static int ExportNormals(
            GeometryDataObject geomData,
            GLTFBinaryData bufferData,
            List<BufferView> bufferViews,
            List<Accessor> accessors,
            MeshPrimitive primitive)
        {
            const string VEC3_STR = "VEC3";
            const string NORMAL_STR = "NORMAL";

            int normalCount = geomData.Normals.Count;
            if (normalCount == 0)
                return -1;

            // Convert normals to float[]
            float[] normals = geomData.Normals
                .Select(n => (float)n)
                .ToArray();

            // Min/Max SOLO para estas normales
            float[] minMax = Util.GetVec3MinMax(normals);

            var max = new List<float> { minMax[1], minMax[3], minMax[5] };
            var min = new List<float> { minMax[0], minMax[2], minMax[4] };

            // Append normals al buffer global (float32)
            int byteOffset = bufferData.AppendFloatArray(normals);
            int byteLength = normals.Length * sizeof(float);

            // Crear bufferView
            BufferView view = new BufferView(
                buffer: 0,
                byteOffset: byteOffset,
                byteLength: byteLength,
                Targets.ARRAY_BUFFER,
                name: ""
            );

            bufferViews.Add(view);
            int viewIdx = bufferViews.Count - 1;

            // Crear accessor
            int count = normalCount / 3;

            Accessor accessor = new Accessor(
                bufferView: viewIdx,
                byteOffset: 0,
                componentType: ComponentType.FLOAT,
                count: count,
                type: VEC3_STR,
                max: max,
                min: min,
                name: NORMAL_STR
            );

            accessors.Add(accessor);
            int accessorIdx = accessors.Count - 1;

            // Asignar al primitive
            primitive.attributes.NORMAL = accessorIdx;

            return accessorIdx;
        }

        public static int ExportBatchId(
            long elementId,
            GeometryDataObject geomData,
            GLTFBinaryData bufferData,
            List<BufferView> bufferViews,
            List<Accessor> accessors,
            MeshPrimitive primitive)
        {
            const string SCALAR_STR = "SCALAR";
            const string BATCH_ID_STR = "_BATCHID";

            int vertexCount = geomData.Vertices.Count / 3;
            if (vertexCount == 0)
                return -1;

            // Crear arreglo de batchId repetido por vértice
            float idValue = (float)elementId;
            float[] batchIds = Enumerable.Repeat(idValue, vertexCount).ToArray();

            // min/max para SCALAR
            var min = new List<float> { idValue };
            var max = new List<float> { idValue };

            // Append batchIds al buffer global
            int byteOffset = bufferData.AppendFloatArray(batchIds);
            int byteLength = batchIds.Length * sizeof(float);

            // Crear bufferView
            BufferView view = new BufferView(
                buffer: 0,
                byteOffset: byteOffset,
                byteLength: byteLength,
                Targets.ARRAY_BUFFER,
                name: ""
            );

            bufferViews.Add(view);
            int viewIdx = bufferViews.Count - 1;

            // Crear accessor
            Accessor accessor = new Accessor(
                bufferView: viewIdx,
                byteOffset: 0,
                componentType: ComponentType.FLOAT,
                count: vertexCount,
                type: SCALAR_STR,
                max: max,
                min: min,
                name: BATCH_ID_STR
            );

            accessors.Add(accessor);
            int accessorIdx = accessors.Count - 1;

            // Guardar resultado en Primitive
            primitive.attributes._BATCHID = accessorIdx;

            return accessorIdx;
        }


        public static int ExportUVs(
            GeometryDataObject geomData,
            GLTFBinaryData bufferData,
            List<BufferView> bufferViews,
            List<Accessor> accessors,
            MeshPrimitive primitive)
        {
            const string VEC2_STR = "VEC2";
            const string TEXCOORD_STR = "TEXCOORD_0";

            int uvCount = geomData.Uvs.Count;
            if (uvCount == 0)
                return -1;

            // Convert (U,V) to float[]
            float[] uvFloats = new float[uvCount * 2];
            int ptr = 0;

            foreach (var uv in geomData.Uvs)
            {
                uvFloats[ptr++] = uv.U;
                uvFloats[ptr++] = uv.V;
            }

            List<float> listUvFloats = new List<float>(uvFloats);
            float[] uvMinMax = Util.GetVec2MinMax(listUvFloats);

            var max = new List<float> { uvMinMax[1], uvMinMax[3] };
            var min = new List<float> { uvMinMax[0], uvMinMax[2] };

            // Append raw bytes to global buffer
            int byteOffset = bufferData.AppendFloatArray(uvFloats);

            int byteLength = uvFloats.Length * sizeof(float);

            // Create bufferView
            BufferView uvBufferView = new BufferView(
                buffer: 0,
                byteOffset: byteOffset,
                byteLength: byteLength,
                Targets.ARRAY_BUFFER,
                name: ""
            );

            bufferViews.Add(uvBufferView);
            int viewIdx = bufferViews.Count - 1;

            // Create accessor
            Accessor accessor = new Accessor(
                bufferView: viewIdx,
                byteOffset: 0,
                componentType: ComponentType.FLOAT,
                count: uvCount,
                type: VEC2_STR,
                max: max,
                min: min,
                name: TEXCOORD_STR
            );

            accessors.Add(accessor);
            int accessorIdx = accessors.Count - 1;

            // Apply to primitive
            primitive.attributes.TEXCOORD_0 = accessorIdx;

            return accessorIdx;
        }

        public static void ExportImageBuffer(
            List<BaseImage> images,
            List<BufferView> bufferViews,
            GLTFBinaryData bufferData)
        {
            foreach (var baseImage in images)
            {
                byte[] imgBytes = baseImage.imageData;
                if (imgBytes == null || imgBytes.Length == 0)
                    continue;

                int byteOffset = bufferData.Append(imgBytes);
                int byteLength = bufferData.byteData.Length - byteOffset;

                BufferView imgView = new BufferView(
                    buffer: 0,   
                    byteOffset: byteOffset,
                    byteLength: byteLength,
                    Targets.NONE,     
                    name: ""
                );

                bufferViews.Add(imgView);
                int viewIdx = bufferViews.Count - 1;


                baseImage.bufferView = viewIdx;
                baseImage.uri = null;
            }
        }

    }
}
