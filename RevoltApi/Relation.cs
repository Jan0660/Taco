using Newtonsoft.Json;

namespace RevoltApi
{
    public class Relation
    {
        [JsonProperty("_id")] public string Id;

        [JsonProperty("status")] public RelationshipStatus Status;
    }
}