namespace Common_glTF_Exporter.Export
{
    using System.Collections.Generic;
    using System.IO;
    using Common_glTF_Exporter.Core;
    using Common_glTF_Exporter.Windows.MainWindow;

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
                int bytePosition = 0;
                int currentBuffer = 0;

                foreach (var view in bufferViews)
                {
                    if (view.buffer.Equals(0))
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
                string bufferUri = string.Concat(preferences.fileName, ".bin");
                buffer.uri = bufferUri;
                buffer.byteLength = bytePosition;
                buffers.Clear();
                buffers.Add(buffer);

                string fileDirectory = string.Concat(preferences.path, ".bin");
                BinFile.Create(fileDirectory, binaryFileData, preferences.normals, preferences.batchId);
        }
    }
}
