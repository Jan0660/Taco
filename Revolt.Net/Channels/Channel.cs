using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Revolt.Channels
{
    public class Channel : RevoltObject
    {
        [JsonProperty("channel_type")] public string ChannelType { get; private set; }

        public Task<SelfMessage> SendMessageAsync(string content, string attachmentId = null)
            => Client.Channels.SendMessageAsync(_id, content, attachmentId == null ? null : new() { attachmentId });

        public async Task<SelfMessage> SendFileAsync(string content, string fileName, string filePath)
        {
            var attachmentId = await Client.UploadFile(fileName, filePath);
            return await Client.Channels.SendMessageAsync(_id, content, new() { attachmentId });
        }

        public async Task<SelfMessage> SendFileAsync(string content, string fileName, byte[] data)
        {
            var attachmentId = await Client.UploadFile(fileName, data);
            return await Client.Channels.SendMessageAsync(_id, content, new() { attachmentId });
        }

        public Task BeginTypingAsync()
            => Client.Channels.BeginTypingAsync(_id);

        public Task EndTypingAsync()
            => Client.Channels.EndTypingAsync(_id);

        public Task<Message[]> GetMessagesAsync(int limit, string before = null, string after = null,
            MessageSort sort = MessageSort.Latest)
            => Client.Channels.GetMessagesAsync(_id, limit, before, after, sort);
    }
}