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
        [JsonIgnore] public string AvatarUrl => Avatar == null ? DefaultAvatarUrl : $"{Client.AutumnUrl}/{Avatar.Tag}/{Avatar._id}/{HttpUtility.UrlEncode(Avatar.Filename)}";

        public Task<RelationshipStatus> AddFriendAsync()
            => Client.Users.AddFriendAsync(Username);

        public Task<RelationshipStatus> RemoveFriendAsync()
            => Client.Users.RemoveFriendAsync(_id);

        public Task<RelationshipStatus> BlockAsync()
            => Client.Users.BlockAsync(_id);

        public Task<RelationshipStatus> UnblockAsync()
            => Client.Users.UnblockAsync(_id);

        public override string ToString()
            => Username;

        public MutualRelationships GetMutualRelationships()
            => Client.Users.GetMutualRelationships(_id);

        public Profile GetProfile()
            => Client.Users.GetProfile(_id);
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