namespace Common_glTF_Exporter.Core
{
    using Revit_glTF_Exporter;

    /// <summary>
    /// A reference to a subsection of a buffer containing either vector or scalar data
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#buffers-and-buffer-views.
    /// </summary>
    public class GLTFBufferView
    {
        /// <summary>
        /// Gets or sets the index of the buffer.
        /// </summary>
        /// <value>
        /// The index of the buffer.
        /// </value>
        public int buffer { get; set; }

        /// <summary>
        /// Gets or sets the offset into the buffer in bytes.
        /// </summary>
        /// <value>
        /// The offset into the buffer in bytes.
        /// </value>
        public int byteOffset { get; set; }

        /// <summary>
        /// Gets or sets the length of the bufferView in bytes.
        /// </summary>
        /// <value>
        /// The length of the bufferView in bytes.
        /// </value>
        public int byteLength { get; set; }

        /// <summary>
        /// Gets or sets the target that the GPU buffer should be bound to.
        /// </summary>
        /// <value>
        /// The target that the GPU buffer should be bound to.
        /// </value>
        public Targets target { get; set; }

        /// <summary>
        /// Gets or sets a user defined name for this view.
        /// </summary>
        /// <value>
        /// A user defined name for this view.
        /// </value>
        public string name { get; set; }
    }

}
