using System.Threading.Tasks;
using Newtonsoft.Json;

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

    public Task<User> FetchPublicBotAsync(string userId)
        => Client._requestAsync<User>($"/bots/{userId}/invite");
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
