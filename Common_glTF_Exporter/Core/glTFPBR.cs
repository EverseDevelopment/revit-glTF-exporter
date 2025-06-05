namespace Common_glTF_Exporter.Core
{
    using System.Collections.Generic;

    public class GLTFPBR
    {
        public List<float> baseColorFactor { get; set; }

        public float metallicFactor { get; set; }

        public float roughnessFactor { get; set; }

        // Texture properties
        public GLTFTextureInfo baseColorTexture { get; set; }
        public GLTFTexture metallicRoughnessTexture { get; set; }
    }
}
