using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB.Visual;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Model;
using Common_glTF_Exporter.Windows.MainWindow;

namespace Common_glTF_Exporter.Export
{
    public static class GlbBinInfo
    {
        public static byte[] Get(GLTFBinaryData globalBuffer)
        {
            GlbBin glbBin = new GlbBin();

            // Bin chunk data = full binary buffer
            glbBin.ChunkData = globalBuffer.byteData ?? new byte[0];
            glbBin.Length = BitConverter.GetBytes((uint)glbBin.ChunkData.Length);

            // Build final chunk: [byteLength][CHUNK_TYPE][binary data]
            byte[] result = new byte[0]
                .Concat(glbBin.Length)
                .Concat(glbBin.ChunkType())
                .Concat(glbBin.ChunkData)
                .ToArray();

            return result;
        }
    }
}
