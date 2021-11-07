using Newtonsoft.Json;
using Revolt.Channels;

namespace Revolt
{
    public class Message : BaseMessage
    {
        [JsonProperty("content")] public string Content { get; internal set; }
        [JsonProperty("replies")] public string[] Replies { get; private set; }
        [JsonProperty("masquerade")] public MessageMasquerade Masquerade { get; private set; }
    }
}