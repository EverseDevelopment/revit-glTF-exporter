﻿namespace Common_glTF_Exporter.Core
{
    using System.Collections.Generic;

    /// <summary>
    /// The scenes available to render
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#scenes.
    /// </summary>
    public class GLTFScene
    {
        public List<int> nodes = new List<int>();
    }
}
