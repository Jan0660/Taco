using Newtonsoft.Json;

namespace Revolt
{
    public class Member
    {
        [JsonProperty("_id")] public MemberId _id { get; internal set; }
        [JsonProperty("nickname")] public string? Nickname { get; internal set; }
        [JsonProperty("avatar")] public Attachment? Avatar { get; internal set; }
        [JsonProperty("roles")] public string[]? Roles { get; internal set; }
    }

    public class MemberId
    {
        [JsonProperty("server")] public string Server { get; set; }
        [JsonProperty("user")] public string User { get; set; }
    }
}