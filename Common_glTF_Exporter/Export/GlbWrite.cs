using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Windows.MainWindow;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace Common_glTF_Exporter.Export
{
    class GlbWrite
    {
        public static void Run(string filename, byte[] header, List<GLTFBinaryData> binaryFileData, 
            bool exportNormals, bool exportBatchId, byte[] json)
        {
            using (FileStream f = File.Create(filename))
            {
                // Write the header to the file
                f.Write(header, 0, header.Length);
                f.Write(json, 12, header.Length);

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
