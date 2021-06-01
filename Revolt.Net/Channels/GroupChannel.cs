using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Revolt.Channels
{
    public class GroupChannel : MessageChannel
    {
        [JsonProperty("name")] public string Name { get; internal set; }
        [JsonProperty("owner")] public string OwnerId { get; internal set; }
        [JsonProperty("description")] public string Description { get; internal set; }

        public Task AddMemberAsync(string userId)
            => Client.Channels.AddGroupMemberAsync(_id, userId);

        public Task RemoveMemberAsync(string userId)
            => Client.Channels.RemoveGroupMemberAsync(_id, userId);

        public Task LeaveAsync()
            => Client.Channels.LeaveAsync(_id);

        public override string ToString()
            => Name;
    }
}