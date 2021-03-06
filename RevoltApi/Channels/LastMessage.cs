using Newtonsoft.Json;

namespace RevoltApi.Channels
{
    public class LastMessage : RevoltObject
    {
        // todo: Author
        [JsonProperty("author")] public string AuthorId;
        [JsonProperty("short")] public string Short;
    }
}