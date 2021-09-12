using Newtonsoft.Json;

namespace Revolt
{
    // todo: rename fields
    public class Session
    {
        [JsonProperty("_id")] public string? Id;
        [JsonProperty("user_id")] public string UserId;
        [JsonProperty("token")] public string SessionToken;
        [JsonProperty("name")] public string Name;
    }
}