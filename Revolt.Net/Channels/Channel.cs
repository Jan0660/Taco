using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Revolt.Channels
{
    public class Channel : RevoltObject
    {
        [JsonProperty("channel_type")] public string ChannelType { get; private set; }

        public Task<SelfMessage> SendMessageAsync(string content, string attachmentId = null)
            => Client.Channels.SendMessageAsync(_id, content, attachmentId);

        public async Task<SelfMessage> SendFileAsync(string content, string fileName, string filePath)
        {
            var attachmentId = await Client.UploadFile(fileName, filePath);
            return await Client.Channels.SendMessageAsync(_id, content, attachmentId);
        }
        
        public async Task<SelfMessage> SendFileAsync(string content, string fileName, byte[] data)
        {
            var attachmentId = await Client.UploadFile(fileName, data);
            return await Client.Channels.SendMessageAsync(_id, content, attachmentId);
        }

        public Task BeginTypingAsync()
            => Client.Channels.BeginTypingAsync(_id);

        public Task EndTypingAsync()
            => Client.Channels.EndTypingAsync(_id);
    }
}