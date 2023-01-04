namespace Common_glTF_Exporter.Core
{
    using System.Collections.Generic;
    using Revit_glTF_Exporter;

    /// <summary>
    /// A reference to a subsection of a BufferView containing a particular data type
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#accessors.
    /// </summary>
    public class GLTFAccessor
    {
        public GLTFAccessor(int bufferView, int byteOffset, ComponentType componentType, int count, string type, List<float> max, List<float> min, string name)
        {
            this.bufferView = bufferView;
            this.byteOffset = byteOffset;
            this.componentType = componentType;
            this.count = count;
            this.type = type;
            this.max = max;
            this.min = min;
            this.name = name;
        }

        /// <summary>
        /// Gets or sets the index of the bufferView.
        /// </summary>
        public int bufferView { get; set; }

        /// <summary>
        /// Gets or sets the offset relative to the start of the bufferView in bytes.
        /// </summary>
        public int byteOffset { get; set; }

        /// <summary>
        /// Gets or sets the datatype of the components in the attribute
        /// </summary>
        public ComponentType componentType { get; set; }

        /// <summary>
        /// Gets or sets the number of attributes referenced by this accessor.
        /// </summary>
        public int count { get; set; }

        /// <summary>
        /// Gets or sets the specifies if the attribute is a scala, vector, or matrix
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Gets or sets the maximum value of each component in this attribute.
        /// </summary>
        public List<float> max { get; set; }

        /// <summary>
        /// Gets or sets the minimum value of each component in this attribute.
        /// </summary>
        public List<float> min { get; set; }

        /// <summary>
        /// Gets or sets a user defined name for this accessor.
        /// </summary>
        public string name { get; set; }
    }
}
