using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Windows.MainWindow;
using glTF.Manipulator.GenericSchema;

namespace Common_glTF_Exporter.Export
{
    internal class GlbFile
    {
        public static void Create(Preferences preferences, GLTFBinaryData binaryFileData, string json)
        {
            byte[] jsonChunk = GlbJsonInfo.Get(json);
            int lenggg = jsonChunk.Length;
            byte[] binChunk = GlbBinInfo.Get(binaryFileData);
            byte[] headerChunk = GlbHeaderInfo.Get(jsonChunk, binChunk);

            string fileDirectory = string.Concat(preferences.path, ".glb");
            byte[] exportArray = headerChunk.Concat(jsonChunk).Concat(binChunk).ToArray();

            File.WriteAllBytes(fileDirectory, exportArray);
        }
    }
}
