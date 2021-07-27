using System.Collections.Generic;
using Newtonsoft.Json;
using NUlid;

namespace Revolt
{
    public class Category
    {
        [JsonProperty("id")] public string Id { get; set; } = Ulid.NewUlid().ToString();
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("channels")] public List<string> Channels { get; set; }
    }
}