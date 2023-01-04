namespace Common_glTF_Exporter.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// The list of accessors available to the renderer for a particular mesh
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#meshes.
    /// </summary>
    public class GLTFAttribute
    {
        /// <summary>
        /// Gets or sets the index of the accessor for position data.
        /// </summary>
        public int POSITION { get; set; }

        /// <summary>
        /// Gets or sets the index of the accessor for normal data.
        /// </summary>
        public int NORMAL { get; set; }

        /// <summary>
        /// Gets or sets the index of the accessor for batchId data.
        /// </summary>
        public int _BATCHID { get; set; }
    }
}
