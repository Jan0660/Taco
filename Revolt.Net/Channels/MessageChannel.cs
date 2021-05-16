using Newtonsoft.Json;

namespace Revolt.Channels
{
    public class MessageChannel : Channel
    {
        [JsonProperty("recipients")] public string[] RecipientIds;
        [JsonProperty("last_message")] public LastMessage LastMessage;
    }
}