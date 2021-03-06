using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RevoltApi
{
    public class Relationship : RevoltObject
    {
        [JsonProperty("_id")] public string UserId;
        [JsonProperty("status")] public RelationshipStatus Status;
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