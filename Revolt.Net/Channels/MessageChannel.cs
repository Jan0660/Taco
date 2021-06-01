using Newtonsoft.Json;

namespace Revolt.Channels
{
    public class MessageChannel : Channel
    {
        [JsonProperty("recipients")] public string[] RecipientIds { get; internal set; }
        [JsonProperty("last_message")] public LastMessage LastMessage { get; internal set; }
    }
}