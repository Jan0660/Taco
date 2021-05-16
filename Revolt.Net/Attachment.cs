using Newtonsoft.Json;

namespace Revolt
{
    public class Attachment : RevoltObject
    {
        [JsonProperty("filename")] public string Filename;
        [JsonProperty("metadata")] public AttachmentMetadata Metadata;
        [JsonProperty("content_type")] public string ContentType;
        [JsonProperty("size")] public ulong Size;
        [JsonProperty("tag")] public string Tag;
    }

    public class AttachmentMetadata
    {
        [JsonProperty("type")] public string Type;
        [JsonProperty("width")] public int? Width;
        [JsonProperty("height")] public int? Height;
    }
}