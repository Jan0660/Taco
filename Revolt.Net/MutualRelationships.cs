using Newtonsoft.Json;

namespace Revolt
{
    public class MutualRelationships
    {
        [JsonProperty("users")] public string[] UserIds { get; internal set; }
    }
}