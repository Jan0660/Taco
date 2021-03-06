using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace RevoltApi
{
    public class RevoltClientUsers
    {
        public RevoltClient Client { get; }
        
        public RevoltClientUsers(RevoltClient client)
        {
            this.Client = client;
        }

        public User Get(string id)
        {
            User user = Client.UsersCache.FirstOrDefault(u => u._id == id);
            if (user != null)
                return user;
            var req = new RestRequest($"/users/{id}");
            var res = Client._restClient.ExecuteGetAsync(req).Result;
            if (res.StatusCode == HttpStatusCode.NotFound)
                return null;
            return Client._deserialize<User>(res.Content);
        }

        public async Task<Relationship[]> GetRelationships()
        {
            var res = await Client._restClient.ExecuteGetAsync(new RestRequest("/users/relationships"));
            var r = JsonConvert.DeserializeObject<Relationship[]>(res.Content);
            foreach (var h in r)
            {
                h.Client = Client;
            }
            return r;
        }

        public async Task<RelationshipStatus> AddFriendAsync(string username)
        {
            var res = await Client._restClient.ExecuteAsync(new RestRequest($"/users/{username}/friend", Method.PUT));
            var user = Client.UsersCache.FirstOrDefault(u => u.Username == username);
            var status = Enum.Parse<RelationshipStatus>(JObject.Parse(res.Content).Value<string>("status"));
            if (user != null)
            {
                user.Relationship = status;
            }

            // todo
            return RelationshipStatus.None;
        }
    }
}