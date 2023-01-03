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
        /// <summary>
        /// Gets or sets the index of the bufferView.
        /// </summary>
        /// <value>
        /// The index of the bufferView.
        /// </value>
        public int bufferView { get; set; }

        /// <summary>
        /// Gets or sets the offset relative to the start of the bufferView in bytes.
        /// </summary>
        /// <value>
        /// The offset relative to the start of the bufferView in bytes.
        /// </value>
        public int byteOffset { get; set; }

        /// <summary>
        /// Gets or sets the datatype of the components in the attribute.
        /// </summary>
        /// <value>
        /// The datatype of the components in the attribute.
        /// </value>
        public ComponentType componentType { get; set; }

        /// <summary>
        /// Gets or sets the number of attributes referenced by this accessor.
        /// </summary>
        /// <value>
        /// The number of attributes referenced by this accessor.
        /// </value>
        public int count { get; set; }

        /// <summary>
        /// Gets or sets the specifies if the attribute is a scala, vector, or matrix.
        /// </summary>
        /// <value>
        /// The specifies if the attribute is a scala, vector, or matrix.
        /// </value>
        public string type { get; set; }

        /// <summary>
        /// Gets or sets the maximum value of each component in this attribute.
        /// </summary>
        /// <value>
        /// The maximum value of each component in this attribute.
        /// </value>
        public List<float> max { get; set; }

        /// <summary>
        /// Gets or sets the minimum value of each component in this attribute.
        /// </summary>
        /// <value>
        /// The minimum value of each component in this attribute.
        /// </value>
        public List<float> min { get; set; }

        /// <summary>
        /// Gets or sets a user defined name for this accessor.
        /// </summary>
        /// <value>
        /// A user defined name for this accessor.
        /// </value>
        public string name { get; set; }
    }
}
