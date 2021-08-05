using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using Revolt.Channels;

namespace Revolt
{
    public class RevoltClientServers
    {
        public RevoltClient Client { get; }

        public RevoltClientServers(RevoltClient client)
        {
            this.Client = client;
        }

        public async Task<TextChannel> CreateChannel(string serverId, string name, string description = null)
        {
            var req = new RestRequest($"/servers/{serverId}/channels");
            req.AddJsonBody(JsonConvert.SerializeObject(new
            {
                name = name,
                description = description,
                nonce = RevoltClient.GenerateNonce()
            }));
            var res = await Client._restClient.ExecutePostAsync(req);
            return (TextChannel)Client._deserializeChannel(res.Content);
        }
        
        public async Task EditServerAsync(string serverId, EditServerRequest request)
        {
            var req = new RestRequest($"/servers/{serverId}");
            req.AddJsonBody(JsonConvert.SerializeObject(request));
            var res = await Client._restClient.ExecuteAsync(req, Method.PATCH);
        }

        public async Task BanUserAsync(string serverId, string userId, string reason)
        {
            var req = new RestRequest($"/servers/{serverId}/bans/{userId}");
            req.AddJsonBody(JsonConvert.SerializeObject(new
            {
                reason
            }));
            var res = await Client._restClient.ExecuteAsync(req, Method.PUT);
        }
    }

    public class EditServerRequest
    {
        [JsonProperty("name")] public string Name { get; set; }
        [JsonProperty("categories")] public Category[] Categories { get; set; }
    }
}