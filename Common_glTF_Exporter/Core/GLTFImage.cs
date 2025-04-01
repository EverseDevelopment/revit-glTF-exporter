namespace Common_glTF_Exporter.Core
{
    /// <summary>
    /// An image used by a texture
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#images
    /// </summary>
    public class GLTFImage
    {
        /// <summary>
        /// The uri of the image
        /// </summary>
        public string uri { get; set; }

        /// <summary>
        /// The image's MIME type
        /// </summary>
        public string mimeType { get; set; }
    }
} 