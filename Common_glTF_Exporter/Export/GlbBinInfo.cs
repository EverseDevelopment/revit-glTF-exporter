using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Model;

namespace Common_glTF_Exporter.Export
{
    public static class GlbBinInfo
    {
        public static byte[] Get(List<GLTFBinaryData> binaryFileData, bool exportNormals, bool exportBatchId)
        {
            List<byte> binData = new List<byte>();

            foreach (var bin in binaryFileData)
            {
                foreach (var coord in bin.vertexBuffer)
                {
                    List<byte> vertex = BitConverter.GetBytes((float)coord).ToList();
                    binData.AddRange(vertex);
                }

                if (exportNormals)
                {
                    foreach (var normal in bin.normalBuffer)
                    {
                        List<byte> normalBuffer = BitConverter.GetBytes((float)normal).ToList();
                        binData.AddRange(normalBuffer);
                    }
                }

                if (bin.uvBuffer != null && bin.uvBuffer.Count > 0)
                {
                    foreach (var uv in bin.uvBuffer)
                    {
                        List<byte> uvBytes = BitConverter.GetBytes((float)uv).ToList();
                        binData.AddRange(uvBytes);
                    }
                }

                if (exportBatchId)
                {
                    foreach (var batchId in bin.batchIdBuffer)
                    {
                        List<byte> batchIdBuffer = BitConverter.GetBytes((float)batchId).ToList();
                        binData.AddRange(batchIdBuffer);
                    }
                }

                foreach (var index in bin.indexBuffer)
                {
                    List<byte> indexIdBuffer = BitConverter.GetBytes((int)index).ToList();
                    binData.AddRange(indexIdBuffer);
                }
            }

            if (binData.Count % 4 != 0)
            {
                int missingNumbers = 4 - (binData.Count % 4);
                for (int i = 0; i < missingNumbers; i++)
                {
                    byte emptyByte = (byte)00;
                    List<byte> zeros = new List<byte> { emptyByte };
                    binData.AddRange(zeros);
                }
            }

            GlbBin glbBin = new GlbBin();
            glbBin.ChunkData = binData.ToArray();
            glbBin.Length = BitConverter.GetBytes(Convert.ToUInt32(glbBin.ChunkData.Length));

            byte[] result = new byte[] { };
            result = result.Concat(glbBin.Length).ToArray();
            result = result.Concat(glbBin.ChunkType()).ToArray();
            result = result.Concat(glbBin.ChunkData).ToArray();

            return result;
        }
    }
}
