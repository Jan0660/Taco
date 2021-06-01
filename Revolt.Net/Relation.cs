using Newtonsoft.Json;

namespace Revolt
{
    public class Relation
    {
        [JsonProperty("_id")] public string Id  { get; internal set; }

        [JsonProperty("status")] public RelationshipStatus Status  { get; internal set; }
    }
}