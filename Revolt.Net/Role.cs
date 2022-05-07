using Newtonsoft.Json;

namespace Revolt
{
    public class Role
    {
        [JsonProperty("name")] public string Name { get; internal set; }

        [JsonProperty("permissions")] public RolePermissions Permissions { get; internal set; }
        [JsonProperty("colour")] public string? Color { get; set; }

        [JsonProperty("hoist")] public bool? Hoist { get; internal set; }
        // todo: do we nullable this?
        [JsonProperty("rank")] public int Rank { get; internal set; }
    }

    public class RolePermissions
    {
        [JsonProperty("a")]
        public long Allow { get; internal set; }
        [JsonProperty("b")]
        public long Deny { get; internal set; }
        [JsonProperty("r")]
        public long? Ranking { get; internal set; }
    }
}