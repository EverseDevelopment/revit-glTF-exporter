namespace Common_glTF_Exporter.Core
{
    using Newtonsoft.Json;
    using Revit_glTF_Exporter;

    /// <summary>
    /// A reference to a subsection of a buffer containing either vector or scalar data.
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#buffers-and-buffer-views
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

        [JsonProperty("buffer")]
        public int buffer { get; set; }

        [JsonProperty("byteOffset")]
        public int byteOffset { get; set; }

        [JsonProperty("byteLength")]
        public int byteLength { get; set; }

        [JsonIgnore] // Ignore the raw enum field so we can customize serialization
        public Targets target { get; set; }

        [JsonProperty("target")]
        public int? TargetValue
        {
            get
            {
                if (target == Targets.ARRAY_BUFFER)
                    return 34962;
                else if (target == Targets.ELEMENT_ARRAY_BUFFER)
                    return 34963;
                else
                    return null;
            }
        }

        public bool ShouldSerializeTargetValue()
        {
            return TargetValue != null;
        }

        [JsonProperty("name")]
        public string name { get; set; }
    }
}
