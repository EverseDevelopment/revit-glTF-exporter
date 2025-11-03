namespace Common_glTF_Exporter.Core
{
    using Common_glTF_Exporter.Utils;
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
        public string generator = string.Concat("e-verse custom generator ", SettingsConfig.currentVersion);
        public string copyright = "free tool created by e-verse";
    }
}
