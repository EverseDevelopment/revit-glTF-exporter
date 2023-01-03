namespace Common_glTF_Exporter.Core
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Revit_glTF_Exporter;

    /// <summary>
    /// The nodes defining individual (or nested) elements in the scene
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#nodes-and-hierarchy.
    /// </summary>
    public class GLTFNode
    {
        /// <summary>
        /// The user-defined name of this object.
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// The index of the mesh in this node.
        /// </summary>
        public int? mesh { get; set; } = null;
        /// <summary>
        /// A floating-point 4x4 transformation matrix stored in column major order.
        /// </summary>
        public List<float> matrix { get; set; }
        /// <summary>
        /// The indices of this node's children.
        /// </summary>
        public List<int> children { get; set; }
        /// <summary>
        /// The extras describing this node.
        /// </summary>
        public GLTFExtras extras { get; set; }
    }
}
