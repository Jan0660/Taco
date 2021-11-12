using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace Revolt;

public class RevoltRestClientBots
{
    public RevoltClient Client { get; }

    public RevoltRestClientBots(RevoltClient client)
    {
        this.Client = client;
    }

    public Task<OwnedBotsResponse> FetchOwnedBotsAsync()
        => Client._requestAsync<OwnedBotsResponse>("/bots/@me");

    public Task<OwnedBotResponse> FetchOwnedBotAsync(string botId)
        => Client._requestAsync<OwnedBotResponse>($"/bots/{botId}");

    public Task DeleteBotAsync(string botId)
        => Client._requestAsync($"/bots/{botId}", Method.DELETE);

    public Task<User> FetchPublicBotAsync(string botId)
        => Client._requestAsync<User>($"/bots/{botId}/invite");
}

public class OwnedBotResponse
{
    [JsonProperty("bot")] public OwnedBotInfo Bot { get; internal set; }
    [JsonProperty("user")] public User User { get; internal set; }
}

public class OwnedBotsResponse
{
    [JsonProperty("bots")] public OwnedBotInfo[] Bots { get; internal set; }
    [JsonProperty("users")] public User[] Users { get; internal set; }
}

public class OwnedBotInfo
{
    [JsonProperty("_id")] public string Id { get; set; }
    [JsonProperty("owner")] public string Owner { get; set; }
    [JsonProperty("token")] public string Token { get; set; }
    [JsonProperty("public")] public bool Public { get; set; }
    [JsonProperty("interactions_url")] public string InteractionsUrl { get; set; }
}