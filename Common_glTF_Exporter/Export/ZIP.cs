namespace Common_glTF_Exporter.Export
{
    using Common_glTF_Exporter.Windows.MainWindow;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;

    public static class ZIP
    {
        /// <summary>
        /// Compress ZIP file.
        /// </summary>
        /// <param name="zipName">ZIP file name.</param>
        /// <param name="files">Files.</param>
        public static void Compress(Preferences preferences)
        {
            List<string> files = new List<string>();
            try
            {
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

                // Validate if there is an existing ZIP
                if (File.Exists(zipFile))
                {
                    File.Delete(zipFile);
                }

                zipAction(zipFile, files);
            }
            finally
            {
                // -- always delete files in finally to ensure the cleanup if some error exists
                files.ForEach(x => File.Delete(x));
            }
        }

        private static void zipAction(string zipName, List<string> files)
        {
            var zip = ZipFile.Open(zipName, ZipArchiveMode.Create);

            foreach (var file in files)
            {
                // Add the entry for each file
                zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
            }

            // Dispose of the object when we are done
            zip.Dispose();
        }
    }
}
