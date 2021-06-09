using Newtonsoft.Json;
using Revolt.Channels;

namespace Revolt
{
    public class Message : BaseMessage
    {
        [JsonProperty("content")] public string Content { get; internal set; }
    }
}