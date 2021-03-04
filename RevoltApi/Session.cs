using Newtonsoft.Json;

namespace RevoltApi
{
    public class Session
    {
        [JsonProperty("id")] public string? Id;
        [JsonProperty("user_id")] public string UserId;
        [JsonProperty("session_token")] public string SessionToken;
    }
}