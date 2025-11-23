using System.Text.Json.Serialization;

namespace Common_glTF_Exporter.Model
{
    public class BaseImage
    {
        [JsonIgnore]
        public string uuid { get; set; }

        [JsonIgnore]
        public byte[] imageData { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string uri { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? bufferView { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string mimeType { get; set; }
    }
}
