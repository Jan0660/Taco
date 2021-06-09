using Newtonsoft.Json;

namespace Revolt
{
    public class ObjectMessage : BaseMessage
    {
        [JsonProperty("content")] public SystemMessage Content { get; internal set; }
    }
}