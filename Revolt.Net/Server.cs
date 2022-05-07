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
        [JsonProperty("roles")] public Dictionary<string, Role>? Roles { get; internal set; }
        [JsonProperty("icon")] public Attachment Icon { get; internal set; }
        [JsonProperty("banner")] public Attachment Banners { get; internal set; }

        // [JsonIgnore]
        // public ServerPermission ServerPermissions
        // {
        //     get => (ServerPermission)DefaultPermissionsRaw[0];
        //     set => DefaultPermissionsRaw[0] = (int)value;
        // }
        //
        // [JsonIgnore]
        // public ChannelPermission ChannelPermissions
        // {
        //     get => (ChannelPermission)DefaultPermissionsRaw[1];
        //     set => DefaultPermissionsRaw[1] = (int)value;
        // }

        [JsonProperty("default_permissions")] public long DefaultPermissionsRaw { get; internal set; }

        [JsonIgnore] public List<Member> MemberCache { get; private set; } = new();

        // public (ServerPermission Server, ChannelPermission Channel) GetPermissionsFor(string userId)
        // {
        //     var member = GetMember(userId);
        //     var serverPerms = ServerPermissions;
        //     var channelPerms = ChannelPermissions;
        //     if (Roles != null)
        //     {
        //         var roles = Roles.Where(r => member.Roles?.Contains(r.Key) ?? false);
        //         foreach (var role in roles)
        //         {
        //             serverPerms |= role.Value.ServerPermissions;
        //             channelPerms |= role.Value.ChannelPermissions;
        //         }
        //     }
        //
        //     return (serverPerms, channelPerms);
        // }

        public Member GetMember(string userId)
            => MemberCache.FirstOrDefault(m => m._id.User == userId) ??
               Client.Servers.FetchMemberAsync(_id, userId).Result;

        public async Task<ServerMembers> GetMembersAsync()
        {
            var members = await Client.Servers.GetMembersAsync(_id);
            MemberCache = members.Members.ToList();
            members.Users.AttachClient(Client);
            Client.CacheUsers(members.Users);
            return members;
        }
    }
}