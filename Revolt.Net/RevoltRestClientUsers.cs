using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Revolt.Channels;

namespace Revolt
{
    public class RevoltRestClientUsers
    {
        public RevoltClient Client { get; }

        public RevoltRestClientUsers(RevoltClient client)
        {
            this.Client = client;
        }

        #region User Information

        public Task<User> FetchUserAsync(string id)
            => Client._requestAsync<User>($"{Client.ApiUrl}/users/{id}");

        public Task<User> FetchSelfAsync()
            => Client._requestAsync<User>($"{Client.ApiUrl}/users/@me");

        /// <summary>
        /// Will be renamed/moved in the future.
        /// </summary>
        public enum EditSelf_Remove
        {
            Avatar,
            ProfileBackground,
            ProfileContent,
            StatusText
        }

        public Task EditSelfAsync(EditUserRequest user)
            => Client._requestAsync($"{Client.ApiUrl}/users/@me", Method.PATCH,
                JsonConvert.SerializeObject(user));

        public Task ChangeUsernameAsync(string username, string password)
            => Client._requestAsync($"{Client.ApiUrl}/users/@me/username", Method.PATCH, JsonConvert.SerializeObject(new
            {
                username, password
            }));

        public Task<Profile> FetchProfileAsync(string id)
            => Client._requestAsync<Profile>($"{Client.ApiUrl}/users/{id}/profile");

        public Task<MutualFriends> FetchMutualFriendsAsync(string id)
            => Client._requestAsync<MutualFriends>($"{Client.ApiUrl}/users/{id}/mutual");

        #endregion

        #region Direct Messaging

        // todo: cache
        // todo: attach client onto Group and DirectMessage icons
        public Task<DirectMessageChannel[]> FetchDirectMessageChannelsAsync()
            => Client._requestAsync<DirectMessageChannel[]>($"{Client.ApiUrl}/users/dms");

        public Task<DirectMessageChannel> OpenDirectMessageChannelAsync(string userId)
            => Client._requestAsync<DirectMessageChannel>($"{Client.ApiUrl}/users/{userId}/dm");

        #endregion

        #region Relationships

        public Task<Relationship[]> FetchRelationshipsAsync()
            => Client._requestAsync<Relationship[]>($"{Client.ApiUrl}/users/relationships");

        public Task<Relationship> FetchRelationshipAsync(string id)
            => Client._requestAsync<Relationship>($"{Client.ApiUrl}/users/{id}/relationship");

        /// <summary>
        /// Send or accept a friend request.
        /// </summary>
        /// <param name="id">User Id.</param>
        public Task<Relationship> FriendAsync(string id)
            => Client._requestAsync<Relationship>($"{Client.ApiUrl}/users/{id}/friend", Method.PUT);

        /// <summary>
        /// Deny a friend request or unfriend someone.
        /// </summary>
        /// <param name="id">User Id.</param>
        public Task<Relationship> UnfriendAsync(string id)
            => Client._requestAsync<Relationship>($"{Client.ApiUrl}/users/{id}/unfriend", Method.DELETE);

        /// <summary>
        /// Block someone.
        /// </summary>
        /// <param name="id">User Id.</param>
        public Task<Relationship> BlockAsync(string id)
            => Client._requestAsync<Relationship>($"{Client.ApiUrl}/users/{id}/block", Method.PUT);

        /// <summary>
        /// Unblock someone.
        /// </summary>
        /// <param name="id">User Id.</param>
        public Task<Relationship> UnblockAsync(string id)
            => Client._requestAsync<Relationship>($"{Client.ApiUrl}/users/{id}/block", Method.DELETE);

        #endregion
    }

    public struct MutualFriends
    {
        [JsonProperty("users")] public string[] Users { get; internal set; }
    }

    public class EditUserRequest
    {
        [JsonProperty("status")] public Status Status { get; set; }
        [JsonProperty("profile")] public Profile Profile { get; set; }
        [JsonProperty("avatar")] public string AvatarId { get; set; }
        [JsonProperty("remove")] public string[] Remove { get; set; }
    }
}