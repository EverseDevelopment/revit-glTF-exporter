namespace Common_glTF_Exporter.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Newtonsoft.Json;
    using Revit_glTF_Exporter;

    /// <summary>
    /// The glTF PBR Material format
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#materials.
    /// </summary>
    public class GLTFMaterial
    {
        public string alphaMode { get; set; }

        public float? alphaCutoff { get; set; }

        public string name { get; set; }

        public GLTFPBR pbrMetallicRoughness { get; set; }

        public bool doubleSided { get; set; }

        public GLTFTexture normalTexture { get; set; }
        public GLTFTexture occlusionTexture { get; set; }
        public GLTFTexture emissiveTexture { get; set; }
        public List<float> emissiveFactor { get; set; }

        [JsonIgnore]
        public string EmbeddedTexturePath { get; set; } = null;
        [JsonIgnore]
        public string UniqueId { get; set; } = null;
    }
}
