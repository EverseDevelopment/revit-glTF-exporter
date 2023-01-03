using Common_glTF_Exporter.Windows.MainWindow;
using Revit_glTF_Exporter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Text;

namespace Common_glTF_Exporter.Export
{
    public static class Binaries
    {
        public static void Save(List<glTFBufferView> bufferViews, List<glTFBuffer> buffers, List<glTFBinaryData> binaryFileData, Preferences preferences) 
        {
            if (preferences.singleBinary)
            {
                int bytePosition = 0;
                int currentBuffer = 0;
                foreach (var view in bufferViews)
                {
                    if (view.buffer == 0)
                    {
                        bytePosition += view.byteLength;
                        continue;
                    }

                    if (view.buffer != currentBuffer)
                    {
                        view.buffer = 0;
                        view.byteOffset = bytePosition;
                        bytePosition += view.byteLength;
                    }
                }

                glTFBuffer buffer = new glTFBuffer();
                string bufferUri = String.Concat(preferences.fileName, ".bin");
                buffer.uri = bufferUri;
                buffer.byteLength = bytePosition;
                buffers.Clear();
                buffers.Add(buffer);

                string fileDirectory = String.Concat(preferences.path, ".bin");
                BinFile.Create(fileDirectory, binaryFileData, preferences.normals, preferences.batchId);
            }
            else
            {
                foreach (var bin in binaryFileData)
                {
                    using (FileStream f = File.Create(preferences.path + bin.name))
                    {
                        using (BinaryWriter writer = new BinaryWriter(f))
                        {
                            foreach (var coord in bin.vertexBuffer)
                            {
                                writer.Write((float)coord);
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
}
