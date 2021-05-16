using Newtonsoft.Json;

namespace Revolt
{
    public class Relation
    {
        [JsonProperty("_id")] public string Id;

        [JsonProperty("status")] public RelationshipStatus Status;
    }
}