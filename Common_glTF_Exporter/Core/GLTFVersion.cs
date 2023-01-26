namespace Common_glTF_Exporter.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Required glTF asset information
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#asset.
    /// </summary>
    public class GLTFVersion
    {
        public string version = "2.0";
        public string generator = "e-verse custom generator";
        public string copyright = "free tool created by e-verse";
    }
}
