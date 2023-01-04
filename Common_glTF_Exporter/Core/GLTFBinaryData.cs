namespace Common_glTF_Exporter.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A binary data store serialized to a *.bin file
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#binary-data-storage.
    /// </summary>
    public class GLTFBinaryData
    {
        public List<float> vertexBuffer { get; set; } = new List<float>();

        public List<float> normalBuffer { get; set; } = new List<float>();

        public List<int> indexBuffer { get; set; } = new List<int>();

        public List<float> batchIdBuffer { get; set; } = new List<float>();

        public int vertexAccessorIndex { get; set; }

        public int normalsAccessorIndex { get; set; }

        public int indexAccessorIndex { get; set; }

        public int batchIdAccessorIndex { get; set; }

        public string name { get; set; }
    }
}
