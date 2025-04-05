using Newtonsoft.Json;

namespace Common_glTF_Exporter.Core
{
    /// <summary>
    /// An image used by a texture
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#images
    /// </summary>
    public class GLTFImage
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string uri { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? bufferView { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string mimeType { get; set; }
    }
} 