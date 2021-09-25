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

        public async Task<ServerMembers> GetMembersAsync(string serverId)
            => (await Client._requestAsync<ServerMembers>($"/servers/{serverId}/members")).CacheUsers(Client);
        
        public async Task EditServerAsync(string serverId, EditServerRequest request)
        {
            var req = new RestRequest($"/servers/{serverId}");
            req.AddJsonBody(JsonConvert.SerializeObject(request));
            var res = await Client._restClient.ExecuteAsync(req, Method.PATCH);
        }
    }

    public class EditServerRequest
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("categories")] public Category[] Categories { get; set; }
    }

    public struct ServerMembers
    {
        [JsonProperty("users")] public User[] Users { get; internal set; }
        [JsonProperty("members")] public Member[] Members { get; internal set; }
    }
}