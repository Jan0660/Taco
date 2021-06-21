using System.Collections.Generic;
using Newtonsoft.Json;

namespace Revolt
{
    public class Server : RevoltObject
    {
        [JsonProperty("nonce")] public string Nonce { get; internal set; }
        [JsonProperty("owner")] public string OwnerId { get; internal set; }
        [JsonProperty("name")] public string Name { get; internal set; }
        [JsonProperty("description")] public string Description { get; internal set; }
        [JsonProperty("channels")] public List<string> ChannelIds { get; internal set; }
        // todo: system_messages
        [JsonProperty("icon")] public Attachment Icon { get; internal set; }
        [JsonProperty("banner")] public Attachment Banners { get; internal set; }
    }
}