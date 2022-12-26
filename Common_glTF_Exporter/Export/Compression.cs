using Common_glTF_Exporter.Windows.MainWindow;
using System.Collections.Generic;
using System.IO;

namespace Common_glTF_Exporter.Export
{
    public class Compression
    {
        public static void Run(string filepath, CompressionEnum compression)
        {
            if (compression.Equals(CompressionEnum.ZIP))
            {
                string gltfFile = string.Concat(filepath, "gltf");
                string binFile = string.Concat(filepath, "bin");
                string zipFile = string.Concat(filepath, "zip");
                List<string> files = new List<string> { gltfFile, binFile };

                ZIP.compress(zipFile, files);
                files.ForEach(x => File.Delete(x));
            }
        }
    }
}
