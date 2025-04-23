using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common_glTF_Exporter.Model;

namespace Common_glTF_Exporter.Export
{
    public static class GlbJsonInfo
    {
        public static byte[] Get(string json)
        {
            GlbJson glbJson = new GlbJson();

            // Convert JSON to UTF-8 byte array (DO THIS FIRST)
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

            // Calculate padding needed to align to 4 bytes
            int padding = (4 - (jsonBytes.Length % 4)) % 4;

            // Allocate new array for padded JSON
            byte[] paddedJsonBytes = new byte[jsonBytes.Length + padding];
            Array.Copy(jsonBytes, paddedJsonBytes, jsonBytes.Length);

            // Pad with spaces (0x20)
            for (int i = jsonBytes.Length; i < paddedJsonBytes.Length; i++)
            {
                paddedJsonBytes[i] = 0x20;
            }

            glbJson.ChunkData = paddedJsonBytes;

            // Write chunk length (uint32)
            glbJson.Length = BitConverter.GetBytes(Convert.ToUInt32(glbJson.ChunkData.Length));

            // Build JSON chunk: length + type + data
            byte[] result = new byte[] { };
            result = result.Concat(glbJson.Length).ToArray();
            result = result.Concat(glbJson.ChunkType()).ToArray(); // Should return 4-byte ASCII for "JSON"
            result = result.Concat(glbJson.ChunkData).ToArray();

            return result;
        }
    }
}
