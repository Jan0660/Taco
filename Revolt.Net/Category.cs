using System.Collections.Generic;
using Newtonsoft.Json;

namespace Revolt
{
    public class Category
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("channels")] public List<string> Channels { get; set; }
    }
}