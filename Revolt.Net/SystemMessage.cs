using Newtonsoft.Json;

namespace Revolt
{
    public class SystemMessage
    {
        [JsonProperty("Type")]
        public string Type { get; internal set; }
        [JsonProperty("content")]
        public string? Content { get; internal set; }
        [JsonProperty("id")]
        public string? Id { get; internal set; }
        [JsonProperty("by")]
        public string? By { get; internal set; }
        [JsonProperty("name")]
        public string? Name { get; internal set; }
    }
}