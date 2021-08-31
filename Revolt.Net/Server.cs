using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Revolt
{
    public class Server : RevoltObject
    {
        [JsonProperty("nonce")] public string Nonce { get; internal set; }
        [JsonProperty("owner")] public string OwnerId { get; internal set; }
        [JsonProperty("name")] public string Name { get; internal set; }
        [JsonProperty("description")] public string Description { get; internal set; }

        [JsonProperty("channels")] public List<string> ChannelIds { get; internal set; }

        // todo: categories
        // todo: system_messages
        [JsonProperty("roles")] public Dictionary<string, Role> Roles { get; internal set; }
        [JsonProperty("icon")] public Attachment Icon { get; internal set; }
        [JsonProperty("banner")] public Attachment Banners { get; internal set; }

        [JsonIgnore]
        public ServerPermission ServerPermissions
        {
            get => (ServerPermission)DefaultPermissionsRaw[0];
            set => DefaultPermissionsRaw[0] = (int)value;
        }

        [JsonIgnore]
        public ChannelPermission ChannelPermissions
        {
            get => (ChannelPermission)DefaultPermissionsRaw[1];
            set => DefaultPermissionsRaw[1] = (int)value;
        }

        [JsonProperty("default_permissions")] public int[] DefaultPermissionsRaw { get; internal set; }

        [JsonIgnore] public List<Member> MemberCache { get; private set; } = new();

        public async Task<ServerMembers> GetMembersAsync()
        {
            var members = await Client.Servers.GetMembersAsync(_id);
            MemberCache = members.Members.ToList();
            // todo: make some magic internal method on client for caching a User that like yes
            return members;
        }
    }
}