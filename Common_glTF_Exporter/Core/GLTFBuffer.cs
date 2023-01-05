namespace Common_glTF_Exporter.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A reference to the location and size of binary data
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#buffers-and-buffer-views.
    /// </summary>
    public class GLTFBuffer
    {
        /// <summary>
        /// Gets or sets the uri of the buffer.
        /// </summary>
        public string uri { get; set; }

        /// <summary>
        /// Gets or sets the total byte length of the buffer.
        /// </summary>
        public int byteLength { get; set; }
    }
}
