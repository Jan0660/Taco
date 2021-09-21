using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using Revolt.Channels;

namespace Revolt
{
    public class RevoltClientServers : RevoltRestClientServers
    {
        public RevoltClient Client { get; }

        public RevoltClientServers(RevoltClient client) : base(client)
        {
            this.Client = client;
        }

        public Task<ServerMembers> GetMembersAsync(string serverId)
            => Client._requestAsync<ServerMembers>($"/servers/{serverId}/members");
    }

    public struct ServerMembers
    {
        [JsonProperty("users")] public User[] Users { get; internal set; }
        [JsonProperty("members")] public Member[] Members { get; internal set; }
    }
}