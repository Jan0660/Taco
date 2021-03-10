using System;
using Newtonsoft.Json;

namespace RevoltApi
{
    public class MessageEditData
    {
        [JsonProperty("content")] public string Content;
        [JsonProperty("edited")] public MessageEditedData Edited;
    }

    public class MessageEditedData
    {
        [JsonProperty("$date")] public DateTime Date;
    }
}