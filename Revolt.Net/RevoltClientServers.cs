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
    }
}