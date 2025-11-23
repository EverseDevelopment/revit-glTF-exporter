using System;
using System.Collections.Generic;
using System.Text;

namespace Common_glTF_Exporter.Model
{
    public class BaseMaterial
    {
        public string uuid { get; set; }
        public string name { get; set; }
        public bool hasTexture { get; set; }
        public int textureIndex { get; set; }
        public bool doubleSided { get; set; }
        public List<float> baseColorFactor { get; set; }
        public float metallicFactor { get; set; }
        public float roughnessFactor { get; set; }
        public string alphaMode { get; set; }
        public float? alphaCutoff { get; set; }
        public float[] offset { get; set; } = new float[] { 0.0f, 0.0f };
        public float[] scale { get; set; } = new float[] { 1.0f, 1.0f };
        public float rotation { get; set; } = 0.0f;
        public int texCoord { get; set; } = 0;
    }
}
