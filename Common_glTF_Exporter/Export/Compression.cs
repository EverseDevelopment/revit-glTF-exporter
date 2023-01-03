namespace Common_glTF_Exporter.Export
{
    using System.Collections.Generic;
    using System.IO;
    using Common_glTF_Exporter.Windows.MainWindow;

    /// <summary>
    /// Compression.
    /// </summary>
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
                string gltfFile = string.Concat(preferences.path, "gltf");
                string binFile = string.Concat(preferences.path, "bin");
                string zipFile = string.Concat(preferences.path, "zip");
                List<string> files = new List<string> { gltfFile, binFile };

                ZIP.Compress(zipFile, files);
                files.ForEach(x => File.Delete(x));
            }
        }
    }
}
