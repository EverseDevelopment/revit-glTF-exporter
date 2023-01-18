using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Windows.MainWindow;

namespace Common_glTF_Exporter.Export
{
    internal class GlbFile
    {
        public static void Create(Preferences preferences, List<GLTFBinaryData> binaryFileData, string json)
        {
            byte[] header = new byte[] {
            0x67, 0x6C, 0x54, 0x46, // Magic number
            0x02, 0x00, 0x00, 0x00, // Version 2
            0x00, 0x00, 0x00, 0x00, // Length of binary body
            };

            byte[] body = new byte[] {
            0x6a, 0x73, 0x6f, 0x6e, // Json
            };

            byte[] jsonBytes = Encoding.ASCII.GetBytes(json);
            int length = 8 + jsonBytes.Length;
            byte[] lengInBytes = Encoding.ASCII.GetBytes(length.ToString());

            byte[] completeBody = lengInBytes.Concat(body).ToArray();
            byte[] completeBodywithContent = completeBody.Concat(jsonBytes).ToArray();


            string fileDirectory = string.Concat(preferences.path, ".glb");
            GlbWrite.Run(fileDirectory, header, binaryFileData, preferences.normals, preferences.batchId, completeBodywithContent);

            //BinFile.Create(fileDirectory, binaryFileData, preferences.normals, preferences.batchId);
        }
    }
}
