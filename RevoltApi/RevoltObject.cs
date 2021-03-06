using Newtonsoft.Json;

namespace RevoltApi
{
    public class RevoltObject
    {
        [JsonProperty("_id")] public string _id;
        [JsonIgnore] public RevoltClient Client;
    }
}