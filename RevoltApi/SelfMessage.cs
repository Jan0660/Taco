using System.Threading.Tasks;
using RestSharp;

namespace RevoltApi
{
    public class SelfMessage : Message
    {
        public Task EditAsync(string newContent)
        {
            var req = new RestRequest($"/channels/{ChannelId}/messages/{_id}");
            req.AddJsonBody(new
            {
                content = newContent
            });
            return Client._restClient.ExecuteAsync(req, Method.PATCH);
        }

        public Task DeleteAsync()
            => Client._restClient.ExecuteAsync(new RestRequest($"/channels/{ChannelId}/messages/{_id}"), Method.DELETE);
    }
}