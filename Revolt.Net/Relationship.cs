using Newtonsoft.Json;

namespace Revolt
{
    public class Relationship : RevoltObject
    {
        [JsonProperty("_id")] public string UserId;
        [JsonProperty("status")] public RelationshipStatus Status  { get; internal set; }
    }
    public enum RelationshipStatus : byte
    {
        None,
        User,
        Friend,
        Outgoing,
        Incoming,
        Blocked,
        BlockedOther
    }
}