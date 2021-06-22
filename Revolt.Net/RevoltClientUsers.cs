using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Revolt
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
            user = Client._deserialize<User>(res.Content);
            user.Client = Client;
            Client._users.Add(user);
            return user;
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
            if (res.Content == "{\"type\":\"NoEffect\"}")
                throw new Exception("No effect.");
            if (res.Content == "{\"type\":\"AlreadyFriends\"}")
                throw new Exception("Already friends.");
            var user = Client.UsersCache.FirstOrDefault(u => u.Username == username);
            var status = (RelationshipStatus)Enum.Parse(typeof(RelationshipStatus),
                JObject.Parse(res.Content).Value<string>("status")!);
            if (user != null)
            {
                user.Relationship = status;
            }

            return status;
        }

        /// <summary>
        /// deletes a friend request or unfriends them
        /// </summary>
        /// <returns></returns>
        public async Task<RelationshipStatus> RemoveFriendAsync(string id)
        {
            var res = await Client._restClient.ExecuteAsync(new RestRequest($"/users/{id}/friend", Method.DELETE));
            if (res.Content == "{\"type\":\"NoEffect\"}")
                throw new Exception("No effect.");
            var user = Client.UsersCache.FirstOrDefault(u => u._id == id);
            var str = JObject.Parse(res.Content).Value<string>("status");
            var status = (RelationshipStatus)Enum.Parse(typeof(RelationshipStatus),
                JObject.Parse(res.Content).Value<string>("status")!);
            if (user != null)
            {
                user.Relationship = status;
            }

            return status;
        }

        public async Task<RelationshipStatus> BlockAsync(string id)
        {
            var res = await Client._restClient.ExecuteAsync(new RestRequest($"/users/{id}/block", Method.PUT));
            if (res.Content == "{\"type\":\"NoEffect\"}")
                throw new Exception("No effect.");
            var user = Client.UsersCache.FirstOrDefault(u => u._id == id);
            var status = (RelationshipStatus)Enum.Parse(typeof(RelationshipStatus),
                JObject.Parse(res.Content).Value<string>("status")!);
            if (user != null)
            {
                user.Relationship = status;
            }

            return status;
        }

        public async Task<RelationshipStatus> UnblockAsync(string id)
        {
            var res = await Client._restClient.ExecuteAsync(new RestRequest($"/users/{id}/block", Method.DELETE));
            if (res.Content == "{\"type\":\"NoEffect\"}")
                throw new Exception("No effect.");
            var user = Client.UsersCache.FirstOrDefault(u => u._id == id);
            var str = JObject.Parse(res.Content).Value<string>("status");
            var status = (RelationshipStatus)Enum.Parse(typeof(RelationshipStatus),
                JObject.Parse(res.Content).Value<string>("status")!);
            if (user != null)
            {
                user.Relationship = status;
            }

            return status;
        }

        public MutualRelationships GetMutualRelationships(string id)
            => JsonConvert.DeserializeObject<MutualRelationships>(
                Client._restClient.ExecuteAsGet(new RestRequest($"/users/{id}/mutual", Method.GET), "GET").Content)!;
        
        public Profile GetProfile(string id)
            => JsonConvert.DeserializeObject<Profile>(
                Client._restClient.ExecuteAsGet(new RestRequest($"/users/{id}/profile", Method.GET), "GET").Content)!;
    }
}