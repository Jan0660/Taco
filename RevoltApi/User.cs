using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RevoltApi
{
    public class User : RevoltObject
    {
        [JsonProperty("username")] public string Username;
        // relations
        // relationship
        [JsonProperty("online")] public bool Online;
        [JsonIgnore] public string DefaultAvatarUrl => $"{Client.ApiUrl}/users/{_id}/default_avatar";
        [JsonIgnore] public string AvatarUrl => $"{Client.ApiUrl}/users/{_id}/avatar";
    }
}