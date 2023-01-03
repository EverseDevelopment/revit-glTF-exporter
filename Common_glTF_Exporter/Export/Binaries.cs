namespace Common_glTF_Exporter.Export
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Policy;
    using System.Text;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Windows.MainWindow;
    using Revit_glTF_Exporter;

    /// <summary>
    /// Binaries.
    /// </summary>
    public static class Binaries
    {
        /// <summary>
        /// Save the binaries.
        /// </summary>
        /// <param name="bufferViews">bufferViews.</param>
        /// <param name="buffers">buffers.</param>
        /// <param name="binaryFileData">binaryFileData.</param>
        /// <param name="preferences">preferences.</param>
        public static void Save(List<GLTFBufferView> bufferViews, List<GLTFBuffer> buffers, List<GLTFBinaryData> binaryFileData, Preferences preferences) 
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

                GLTFBuffer buffer = new GLTFBuffer();
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
