using Newtonsoft.Json;

namespace RevoltApi
{
    public class Attachment : RevoltObject
    {
        [JsonProperty("filename")] public string Filename;
        [JsonProperty("metadata")] public AttachmentMetadata Metadata;
        [JsonProperty("content_type")] public string ContentType;
    }

    public class AttachmentMetadata
    {
        [JsonProperty("type")] public string Type;
        [JsonProperty("width")] public int? Width;
        [JsonProperty("height")] public int? Height;
    }
}