namespace Revit_glTF_Exporter
{
    using System.Collections.Generic;
    using Common_glTF_Exporter.Core;

    /// <summary>
    /// Magic numbers to differentiate scalar and vector.
    /// array buffers.
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#buffers-and-buffer-views.
    /// </summary>
    public enum Targets
    {
        ARRAY_BUFFER = 34962, // signals vertex data
        ELEMENT_ARRAY_BUFFER = 34963, // signals index or face data
    }

    /// <summary>
    /// Magic numbers to differentiate array buffer component types.
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#accessor-element-size.
    /// </summary>
    public enum ComponentType
    {
        BYTE = 5120,
        UNSIGNED_BYTE = 5121,
        SHORT = 5122,
        UNSIGNED_SHORT = 5123,
        UNSIGNED_INT = 5125,
        FLOAT = 5126,
    }

    /// <summary>
    /// The json serializable glTF file format
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0.
    /// </summary>
    public struct GLTF
    {
        public GLTFVersion asset;
        public List<GLTFScene> scenes;
        public List<GLTFNode> nodes;
        public List<GLTFMesh> meshes;
        public List<GLTFBuffer> buffers;
        public List<GLTFBufferView> bufferViews;
        public List<GLTFAccessor> accessors;
        public List<GLTFMaterial> materials;
    }
}
