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

        public Task BanMemberAsync(string id, string userId)
            => Client._requestAsync($"{Client.ApiUrl}/servers/{id}/bans/{userId}", Method.PUT);

        public Task UnbanMemberAsync(string id, string userId)
            => Client._requestAsync($"{Client.ApiUrl}/servers/{id}/bans/{userId}", Method.DELETE);

        #endregion

        #region Server Permissions

        public Task SetRolePermission(string id, string roleId, ServerPermissions permissions)
            => Client._requestAsync($"{Client.ApiUrl}/servers/{id}/permissions/{roleId}", Method.PUT,
                JsonConvert.SerializeObject(new { permissions }));

        /// <summary>
        /// Set permissions for the default role.
        /// </summary>
        /// <param name="id">Server Id</param>
        /// <param name="permissions">The permissions</param>
        public Task SetDefaultPermission(string id, ServerPermissions permissions)
            => Client._requestAsync($"{Client.ApiUrl}/servers/{id}/permissions/default", Method.PUT,
                JsonConvert.SerializeObject(new { permissions }));

        #endregion
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