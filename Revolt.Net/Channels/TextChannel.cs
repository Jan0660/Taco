using Newtonsoft.Json;

namespace Revolt.Channels
{
    public class TextChannel : Channel
    {
        [JsonProperty("server")] public string ServerId;
        [JsonProperty] public string Name { get; internal set; }
        [JsonProperty] public string Description { get; internal set; }
        [JsonProperty] public Attachment? Icon { get; internal set; }
        [JsonProperty("last_message")] public string LastMessage { get; internal set; }
    }
}