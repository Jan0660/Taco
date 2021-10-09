using System.Web;
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
        [JsonIgnore] public string Url => $"{Client.AutumnUrl}/{Tag}/{_id}/{HttpUtility.UrlEncode(Filename)}";
    }

    public class AttachmentMetadata
    {
        [JsonProperty("type")] public string Type { get; private set; }
        [JsonProperty("width")] public long? Width { get; private set; }
        [JsonProperty("height")] public long? Height { get; private set; }
    }
}