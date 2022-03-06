using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using Revolt.Channels;

namespace Revolt
{
    public class RevoltRestClientServers
    {
        public RevoltClient Client { get; }

        public RevoltRestClientServers(RevoltClient client)
        {
            this.Client = client;
        }

        #region Server Information

        /// <summary>
        /// Leave a server or, if you are the owner of it, delete it.
        /// </summary>
        /// <param name="id">Server Id</param>
        public Task LeaveServerAsync(string id)
            => Client._requestAsync($"{Client.ApiUrl}/servers/{id}", Method.DELETE);

        public Task<Server> CreateServerAsync(CreateServerRequest server)
        {
            server.Nonce ??= RevoltClient.GenerateNonce();
            return Client._requestAsync<Server>("/servers/create", Method.POST,
                JsonConvert.SerializeObject(server));
        }

        public Task<Channel> CreateChannelAsync(string id, CreateChannelRequest channel)
        {
            channel.Nonce ??= RevoltClient.GenerateNonce();
            return Client._requestAsync<Channel>($"/servers/{id}/channels", Method.POST,
                JsonConvert.SerializeObject(channel));
        }

        public Task MarkAsReadAsync(string id)
            => Client._requestAsync($"{Client.ApiUrl}/servers/{id}/ack", Method.PUT);

        #endregion

        #region Server Members

        public Task<Member> FetchMemberAsync(string id, string userId)
            => Client._requestAsync<Member>($"{Client.ApiUrl}/servers/{id}/members/{userId}");

        public Task KickMemberAsync(string id, string userId)
            => Client._requestAsync($"{Client.ApiUrl}/servers/{id}/members/{userId}", Method.DELETE);

        public Task<IRestResponse> BanMemberAsync(string id, string userId, string? reason = null)
            => Client._requestAsync($"{Client.ApiUrl}/servers/{id}/bans/{userId}", Method.PUT,
                JsonConvert.SerializeObject(new
                {
                    reason
                }));

        public Task<IRestResponse> UnbanMemberAsync(string id, string userId)
            => Client._requestAsync($"{Client.ApiUrl}/servers/{id}/bans/{userId}", Method.DELETE);

        #endregion

        #region Server Permissions

        public Task SetRolePermissionAsync(string id, string roleId, ServerPermissions permissions)
            => Client._requestAsync($"{Client.ApiUrl}/servers/{id}/permissions/{roleId}", Method.PUT,
                JsonConvert.SerializeObject(new { permissions }));

        /// <summary>
        /// Set permissions for the default role.
        /// </summary>
        /// <param name="id">Server Id</param>
        /// <param name="permissions">The permissions</param>
        public Task SetDefaultPermissionAsync(string id, ServerPermissions permissions)
            => Client._requestAsync($"{Client.ApiUrl}/servers/{id}/permissions/default", Method.PUT,
                JsonConvert.SerializeObject(new { permissions }));

        public Task<CreateRoleResponse> CreateRoleAsync(string id, string name)
            => Client._requestAsync<CreateRoleResponse>($"{Client.ApiUrl}/servers/{id}/roles", Method.POST,
                JsonConvert.SerializeObject(new { name }));
        
        public Task EditRoleAsync(string id, string roleId, EditRoleRequest request)
            => Client._requestAsync($"{Client.ApiUrl}/servers/{id}/roles/{roleId}", Method.PATCH, JsonConvert.SerializeObject(request));
        
        public Task DeleteRoleAsync(string id, string roleId)
            => Client._requestAsync($"{Client.ApiUrl}/servers/{id}/roles/{roleId}", Method.DELETE);

        #endregion
    }

    public class EditRoleRequest
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("colour")] public string Colour { get; set; }
        [JsonProperty("hoist")] public bool Hoist { get; set; }
        [JsonProperty("rank")] public int Rank { get; set; }
        /// <summary>
        /// "Colour"
        /// </summary>
        [JsonProperty("remove")] public string[] Remove { get; set; }
    }

    public class CreateRoleResponse
    {
        [JsonProperty("id")] public string Id { get; private set; }
        [JsonProperty("permissions")] public int[] PermissionsRaw { get; private set; }
        [JsonIgnore] public ServerPermission ServerPermissions => (ServerPermission)PermissionsRaw[0];
        [JsonIgnore] public ChannelPermission ChannelPermissions => (ChannelPermission)PermissionsRaw[1];
    }

    public class CreateServerRequest
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string? Description { get; set; }
        [JsonProperty("nsfw")] public bool Nsfw { get; set; }
        [JsonProperty("nonce")] public string? Nonce { get; set; }
    }

    public class CreateChannelRequest
    {
        // todo: enum
        [JsonProperty("type")] public string Type { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("description")] public string? Description { get; set; }
        [JsonProperty("nsfw")] public bool Nsfw { get; set; }
        [JsonProperty("nonce")] public string? Nonce { get; set; }
    }

    public struct ServerPermissions
    {
        [JsonIgnore]
        public ServerPermission Server
        {
            get => (ServerPermission)ServerRaw;
            set => ServerRaw = (int)value;
        }

        [JsonProperty("server")] public int ServerRaw { get; set; }

        [JsonIgnore]
        public ChannelPermission Channel
        {
            get => (ChannelPermission)ChannelRaw;
            set => ChannelRaw = (int)value;
        }

        [JsonProperty("channel")] public int ChannelRaw { get; set; }
    }
}