namespace Common_glTF_Exporter.Core
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Texture information for a material property
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#texture-info
    /// </summary>
    public class GLTFTexture
    {
        public int source { get; set; }
    }

    public class GLTFTextureInfo
    {
        public int index { get; set; }
        public int texCoord { get; set; } = 0;
    }
} 