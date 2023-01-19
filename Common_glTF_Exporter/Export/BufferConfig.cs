using System;
using System.Collections.Generic;
using System.Text;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Windows.MainWindow;

namespace Common_glTF_Exporter.Export
{
    public static class BufferConfig
    {
        public static void Run(List<GLTFBufferView> bufferViews, List<GLTFBuffer> buffers, 
            Preferences preferences)
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

            if (preferences.format == FormatEnum.gltf)
            {
                string bufferUri = string.Concat(preferences.fileName, ".bin");
                buffer.uri = bufferUri;
            }

            buffer.byteLength = bytePosition;
            buffers.Clear();
            buffers.Add(buffer);
        }
    }
}
