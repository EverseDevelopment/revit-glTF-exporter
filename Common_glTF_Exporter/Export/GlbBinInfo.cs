using System;
using Common_glTF_Exporter.Model;
using glTF.Manipulator.GenericSchema;

namespace Common_glTF_Exporter.Export
{
    public static class GlbBinInfo
    {
        public static byte[] Get(GLTFBinaryData globalBuffer)
        {
            GlbBin glbBin = new GlbBin();

            // Usar MemoryStream → ToArray()
            glbBin.ChunkData = globalBuffer.ToArray();

            // Length del chunk BIN
            glbBin.Length = BitConverter.GetBytes((uint)glbBin.ChunkData.Length);

            // 📌 Construcción del chunk final:
            // [byteLength][CHUNK_TYPE][binary data]
            byte[] result = new byte[
                  glbBin.Length.Length
                + glbBin.ChunkType().Length
                + glbBin.ChunkData.Length];

            int offset = 0;

            // Copiar chunk length
            Buffer.BlockCopy(glbBin.Length, 0, result, offset, glbBin.Length.Length);
            offset += glbBin.Length.Length;

            // Copiar chunk type
            byte[] type = glbBin.ChunkType();
            Buffer.BlockCopy(type, 0, result, offset, type.Length);
            offset += type.Length;

            // Copiar datos binarios
            Buffer.BlockCopy(glbBin.ChunkData, 0, result, offset, glbBin.ChunkData.Length);

            return result;
        }
    }
}
