namespace Common_glTF_Exporter.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Windows.MainWindow;

    public static class BinFile
    {
        /// <summary>
        /// Create a new .bin file.
        /// </summary>
        /// <param name="filename">.bin file name.</param>
        /// <param name="binaryFileData">binary file data.</param>
        /// <param name="exportNormals">export normals.</param>
        /// <param name="exportBatchId">export BatchId.</param>
        public static void Create(string filename, GLTFBinaryData globalBuffer)
        {
            using (FileStream f = File.Create(filename))
            using (var writer = new BinaryWriter(new BufferedStream(f)))
            {
                if (globalBuffer.byteData != null && globalBuffer.byteData.Length > 0)
                {
                    writer.Write(globalBuffer.byteData);
                }

                writer.Flush();
            }
        }
    }
}
