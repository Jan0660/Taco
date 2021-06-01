using Newtonsoft.Json;

namespace Revolt
{
    public class Attachment : RevoltObject
    {
        [JsonProperty("filename")] public string Filename { get; private set; }
        [JsonProperty("metadata")] public AttachmentMetadata Metadata { get; internal set; }
        [JsonProperty("content_type")] public string ContentType { get; private set; }
        [JsonProperty("size")] public ulong Size { get; private set; }
        [JsonProperty("tag")] public string Tag { get; private set; }
    }

    public class AttachmentMetadata
    {
        [JsonProperty("type")] public string Type { get; private set; }
        [JsonProperty("width")] public int? Width { get; private set; }
        [JsonProperty("height")] public int? Height { get; private set; }
    }
}