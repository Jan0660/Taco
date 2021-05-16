using Newtonsoft.Json;

namespace Revolt.Channels
{
    public class DirectMessageChannel : MessageChannel
    {
        [JsonProperty("active")] public bool Active;
    }
}