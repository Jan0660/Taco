using Newtonsoft.Json;

namespace RevoltApi.Channels
{
    public class DirectMessageChannel : MessageChannel
    {
        [JsonProperty("active")] public bool Active;
    }
}