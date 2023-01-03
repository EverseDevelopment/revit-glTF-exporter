namespace Common_glTF_Exporter.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using Common_glTF_Exporter.Core;
    using Revit_glTF_Exporter;

    /// <summary>
    /// Bin file.
    /// </summary>
    public static class BinFile
    {
        /// <summary>
        /// Create a new .bin file.
        /// </summary>
        /// <param name="filename">.bin file name.</param>
        /// <param name="binaryFileData">binary file data.</param>
        /// <param name="exportNormals">export normals.</param>
        /// <param name="exportBatchId">export BatchId.</param>
        public static void Create(string filename, List<GLTFBinaryData> binaryFileData, bool exportNormals, bool exportBatchId)
        {
            using (FileStream f = File.Create(filename))
            {
                using (BinaryWriter writer = new BinaryWriter(f))
                {
                    foreach (var bin in binaryFileData)
                    {
                        foreach (var coord in bin.vertexBuffer)
                        {
                            writer.Write((float)coord);
                        }

                        if (exportNormals)
                        {
                            foreach (var normal in bin.normalBuffer)
                            {
                                writer.Write((float)normal);
                            }
                        }

                        if (exportBatchId)
                        {
                            foreach (var batchId in bin.batchIdBuffer)
                            {
                                writer.Write((float)batchId);
                            }
                        }

                        foreach (var index in bin.indexBuffer)
                        {
                            writer.Write((int)index);
                        }
                    }
                }
            }
        }
    }
}
