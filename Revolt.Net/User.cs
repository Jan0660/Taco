using System;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace Revolt
{
    public class User : RevoltObject
    {
        [JsonProperty("username")] public string Username { get; internal set; }
        [JsonProperty("relations")] public Relation[] Relations { get; internal set; }
        [JsonProperty("status")] public Status Status { get; internal set; }
        [JsonIgnore] public Badges Badges => (Badges)BadgesRaw;
        [JsonProperty("badges")] public int BadgesRaw { get; internal set; }
        [JsonProperty("relationship")] public RelationshipStatus Relationship { get; internal set; }
        [JsonProperty("online")] public bool Online { get; internal set; }
        [JsonProperty("avatar")] public Attachment? Avatar { get; internal set; }
        [JsonProperty("bot")] public UserBot? Bot { get; internal set; }
        [JsonIgnore] public string DefaultAvatarUrl => $"{Client.ApiUrl}/users/{_id}/default_avatar";
        /// <summary>
        /// Gets URL to user's avatar, if they don't have one, falls back to <see cref="DefaultAvatarUrl"/>.
        /// </summary>
        [JsonIgnore] public string AvatarUrl => Avatar == null ? DefaultAvatarUrl : Avatar.Url;

        public Task FriendAsync()
            => Client.Users.FriendAsync(Username);

        public Task UnfriendAsync()
            => Client.Users.UnfriendAsync(_id);

        public Task BlockAsync()
            => Client.Users.BlockAsync(_id);

        public Task UnblockAsync()
            => Client.Users.UnblockAsync(_id);

        public override string ToString()
            => Username;

        public Task<MutualFriends> FetchMutualFriendsAsync()
            => Client.Users.FetchMutualFriendsAsync(_id);

        public Task<Profile> FetchProfileAsync()
            => Client.Users.FetchProfileAsync(_id);
        // todo: more methods things
    }

    public class UserBot
    {
        [JsonProperty("owner")]
        public string OwnerId { get; internal set; }
    }

    [Flags]
    public enum Badges
    {
        Developer = 1,
        Translator = 2,
        Supporter = 4,
        ResponsibleDisclosure = 8,
        EarlyAdopter = 256
    }
}