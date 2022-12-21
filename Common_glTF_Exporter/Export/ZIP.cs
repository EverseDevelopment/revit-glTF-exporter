using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Common_glTF_Exporter.Export
{
    public static class ZIP
    {
        public static void compress(string zipName, List<string> files)
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
