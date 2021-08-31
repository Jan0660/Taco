using Newtonsoft.Json;

namespace Revolt
{
    public class Role
    {
        [JsonProperty("name")] public string Name { get; internal set; }

        [JsonIgnore]
        public ServerPermission ServerPermissions
        {
            get => (ServerPermission)PermissionsRaw[0];
            set => PermissionsRaw[0] = (int)value;
        }

        [JsonIgnore]
        public ChannelPermission ChannelPermissions
        {
            get => (ChannelPermission)PermissionsRaw[1];
            set => PermissionsRaw[1] = (int)value;
        }

        [JsonProperty("permissions")] public int[] PermissionsRaw { get; internal set; }
        [JsonProperty("colour")] public string? Color { get; set; }

        [JsonProperty("hoist")] public string? Hoist { get; internal set; }
        [JsonProperty("rank")] public int Rank { get; internal set; }
    }
}