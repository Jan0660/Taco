using System;
using Newtonsoft.Json;

namespace Revolt
{
    public class MessageEditData
    {
        [JsonProperty("content")] public string Content { get; internal set; }
        [JsonProperty("edited")] public MessageEditedData Edited { get; internal set; }
    }

    public class MessageEditedData
    {
        [JsonProperty("$date")] public DateTime Date { get; internal set; }
    }
}