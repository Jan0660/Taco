using Newtonsoft.Json;

namespace Revolt
{
    public class RevoltObject
    {
        // ReSharper disable once InconsistentNaming
        [JsonProperty("_id")] public string _id;
        [JsonIgnore] public RevoltClient Client;
    }
}