namespace Common_glTF_Exporter.Export
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using Common_glTF_Exporter.Core;

    public static class BinFile
    {
        /// <summary>
        /// Create a new .bin file.
        /// </summary>
        /// <param name="filename">.bin file name.</param>
        /// <param name="binaryFileData">binary file data.</param>
        /// <param name="exportNormals">export normals.</param>
        /// <param name="exportBatchId">export BatchId.</param>
        public static void Create(string filename, List<GLTFBinaryData> binaryFileData, 
            bool exportNormals, bool exportBatchId)
        {
            using (FileStream f = File.Create(filename))
            using (var writer = new BinaryWriter(new BufferedStream(f), Encoding.Default))
            {
                foreach (var bin in binaryFileData)
                {
                    for (int i = 0; i < bin.vertexBuffer.Count; i++)
                    {
                        writer.Write((float)bin.vertexBuffer[i]);
                    }

                    if (exportNormals)
                    {
                        for (int i = 0; i < bin.normalBuffer.Count; i++)
                        {
                            writer.Write((float)bin.normalBuffer[i]);
                        }
                    }

                    if (exportBatchId)
                    {
                        for (int i = 0; i < bin.batchIdBuffer.Count; i++)
                        {
                            writer.Write((float)bin.batchIdBuffer[i]);
                        }
                    }

                    for (int i = 0; i < bin.indexBuffer.Count; i++)
                    {
                        writer.Write((int)bin.indexBuffer[i]);
                    }
                }

                writer.Flush();
            }
        }
    }
}
