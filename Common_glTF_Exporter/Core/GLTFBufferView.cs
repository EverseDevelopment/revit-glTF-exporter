namespace Common_glTF_Exporter.Core
{
    using Revit_glTF_Exporter;

    /// <summary>
    /// A reference to a subsection of a buffer containing either vector or scalar data
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#buffers-and-buffer-views.
    /// </summary>
    public class GLTFBufferView
    {
        public GLTFBufferView(int buffer, int byteOffset, int byteLength, Targets target, string name)
        {
            this.buffer = buffer;
            this.byteOffset = byteOffset;
            this.byteLength = byteLength;
            this.target = target;
            this.name = name;
        }

        /// <summary>
        /// Gets or sets the index of the buffer.
        /// </summary>
        public int buffer { get; set; }

        /// <summary>
        /// Gets or sets the offset into the buffer in bytes.
        /// </summary>
        public int byteOffset { get; set; }

        /// <summary>
        /// Gets or sets the length of the bufferView in bytes.
        /// </summary>
        public int byteLength { get; set; }

        /// <summary>
        /// Gets or sets the target that the GPU buffer should be bound to.
        /// </summary>
        public Targets target { get; set; }

        /// <summary>
        /// Gets or sets a user defined name for this view.
        /// </summary>
        public string name { get; set; }
    }
}
