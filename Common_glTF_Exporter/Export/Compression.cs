namespace Common_glTF_Exporter.Export
{
    using System.Collections.Generic;
    using System.IO;
    using Common_glTF_Exporter.Windows.MainWindow;

    public class Compression
    {
        /// <summary>
        /// Run compression.
        /// </summary>
        /// <param name="preferences">preferences.</param>
        public static void Run(Preferences preferences)
        {
            if (preferences.compression.Equals(CompressionEnum.ZIP))
            {
                List<string> files = new List<string>();
                string zipFile = string.Concat(preferences.path, ".zip");

                if (preferences.format == FormatEnum.gltf)
                {
                    string gltfFile = string.Concat(preferences.path, ".gltf");
                    string binFile = string.Concat(preferences.path, ".bin");

                    files.Add(gltfFile);
                    files.Add(binFile);
                }
                else
                {
                    string glbFile = string.Concat(preferences.path, ".glb");
                    files.Add(glbFile);
                }

                // TODO: Validate if there is an existing ZIP
                ZIP.Compress(zipFile, files);
                files.ForEach(x => File.Delete(x));
            }
        }
    }
}
