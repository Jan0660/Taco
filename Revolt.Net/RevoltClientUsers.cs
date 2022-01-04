using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Revolt
{
    public class RevoltClientUsers : RevoltRestClientUsers
    {
        public RevoltClient Client { get; }

        public RevoltClientUsers(RevoltClient client) : base(client)
        {
            this.Client = client;
        }

        /// <summary>
        /// Gets a user from cache or fetches them.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <returns>The user.</returns>
        public User Get(string id)
        {
            User user = Client.UsersCache.FirstOrDefault(u => u._id == id);
            if (user != null)
                return user;
            user = FetchUserAsync(id).Result;
            Client._users.TryAdd(user._id, user);
            return user;
        }

        /// <summary>
        /// Gets a user from cache.
        /// </summary>
        /// <param name="id">User ID.</param>
        /// <returns>The user.</returns>
        public User? GetCached(string id)
            => Client.UsersCache.FirstOrDefault(u => u._id == id);
    }
}