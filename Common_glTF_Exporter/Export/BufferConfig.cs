using System;
using System.Collections.Generic;
using Common_glTF_Exporter.Core;
using Common_glTF_Exporter.Windows.MainWindow;
using glTF.Manipulator.Schema;
using Buffer = glTF.Manipulator.Schema.Buffer;

namespace Common_glTF_Exporter.Export
{
    public static class BufferConfig
    {
        const string BIN = ".bin";

        public static void Run(List<BufferView> bufferViews, List<Buffer> buffers,
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

            Buffer buffer = new Buffer();

            if (preferences.format == FormatEnum.gltf)
            {
                string bufferUri = string.Concat(preferences.fileName, BIN);
                buffer.uri = bufferUri;
            }

            buffer.byteLength = bytePosition;
            buffers.Clear();
            buffers.Add(buffer);
        }
    }
}
