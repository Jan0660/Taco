using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RevoltApi
{
    public class User : RevoltObject
    {
        [JsonProperty("username")] public string Username;
        [JsonProperty("relations")] public Relation[] Relations;
        [JsonProperty("relationship")] public RelationshipStatus Relationship;
        [JsonProperty("online")] public bool Online;
        [JsonIgnore] public string DefaultAvatarUrl => $"{Client.ApiUrl}/users/{_id}/default_avatar";
        [JsonIgnore] public string AvatarUrl => $"{Client.ApiUrl}/users/{_id}/avatar";

        public Task<RelationshipStatus> AddFriendAsync()
            => Client.Users.AddFriendAsync(Username);
        
        public Task<RelationshipStatus> RemoveFriendAsync()
            => Client.Users.RemoveFriendAsync(_id);
        
        public Task<RelationshipStatus> BlockAsync()
            => Client.Users.BlockAsync(_id);
        
        public Task<RelationshipStatus> UnblockAsync()
            => Client.Users.UnblockAsync(_id);
    }
}