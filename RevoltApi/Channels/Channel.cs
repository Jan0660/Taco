using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RevoltApi.Channels
{
    public class Channel : RevoltObject
    {
        [JsonProperty("channel_type")] public string ChannelType;

        public Task<Message> SendMessageAsync(string content, string attachmentId = null)
            => Client.SendMessageToChannel(_id, content, attachmentId);

        public async Task<Message> SendFileAsync(string content, string fileName, string filePath)
        {
            var attachmentId = await Client.UploadFile(fileName, filePath);
            return await Client.SendMessageToChannel(_id, content, attachmentId);
        }
        
        public async Task<Message> SendFileAsync(string content, string fileName, byte[] data)
        {
            var attachmentId = await Client.UploadFile(fileName, data);
            return await Client.SendMessageToChannel(_id, content, attachmentId);
        }

        public Task BeginTypingAsync()
            => Client.BeginTypingAsync(_id);

        public Task EndTypingAsync()
            => Client.EndTypingAsync(_id);
    }
}