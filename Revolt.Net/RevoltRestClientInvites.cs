using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace Revolt;

public class RevoltRestClientInvites
{
    public RevoltClient Client { get; }

    public RevoltRestClientInvites(RevoltClient client)
    {
        this.Client = client;
    }

    public Task<InviteInfo> FetchInviteAsync(string inviteId)
        => Client._requestAsync<InviteInfo>($"/invites/{inviteId}");

    public Task JoinInviteAsync(string inviteId)
        => Client._requestAsync($"/invites/{inviteId}", Method.POST);

    public Task DeleteInviteAsync(string inviteId)
        => Client._requestAsync($"/invites/{inviteId}", Method.DELETE);
}

public class InviteInfo
{
    [JsonProperty("type")] public string Type { get; set; }
    [JsonProperty("server_id")] public string ServerId { get; set; }
    [JsonProperty("server_name")] public string ServerName { get; set; }
    [JsonProperty("server_icon")] public Attachment ServerIcon { get; set; }
    [JsonProperty("server_banner")] public Attachment ServerBanner { get; set; }
    [JsonProperty("channel_id")] public string ChannelId { get; set; }
    [JsonProperty("channel_name")] public string ChannelName { get; set; }
    [JsonProperty("channel_description")] public string ChannelDescription { get; set; }
    [JsonProperty("user_name")] public string UserName { get; set; }
    [JsonProperty("user_avatar")] public Attachment UserAvatar { get; set; }
    [JsonProperty("member_count")] public int MemberCount { get; set; }
}