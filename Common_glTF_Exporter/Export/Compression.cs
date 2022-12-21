using Common_glTF_Exporter.Windows.MainWindow;
using System.Collections.Generic;
using System.IO;

namespace Common_glTF_Exporter.Export
{
    public class Compression
    {
        public static void Run(string filename, CompressionEnum compression)
        {
            if (compression.Equals(CompressionEnum.ZIP))
            {
                string bufferUri = filename.Replace("gltf", "bin");
                string zipName = filename.Replace("gltf", "zip");
                List<string> files = new List<string> { filename, bufferUri };

                ZIP.compress(zipName, files);
                File.Delete(filename);
                File.Delete(filename.Replace("gltf", "bin"));
            }
        }
    }
}
