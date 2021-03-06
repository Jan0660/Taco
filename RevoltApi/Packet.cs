using Newtonsoft.Json;

namespace RevoltApi
{
    public class Packet
    {
        [JsonProperty("type")] public string Type;
    }
}