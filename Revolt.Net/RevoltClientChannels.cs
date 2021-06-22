using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using Revolt.Channels;

namespace Revolt
{
    public class RevoltClientChannels
    {
        public RevoltClient Client { get; }

        public RevoltClientChannels(RevoltClient client)
        {
            this.Client = client;
        }


        public Channel Get(string id)
        {
            Channel channel = Client.ChannelsCache.FirstOrDefault(u => u._id == id);
            if (channel != null)
                return channel;
            return Client._deserializeChannel(Client._restClient.Get(new RestRequest($"/channels/{id}/")).Content);
        }

        public async Task<SelfMessage> SendMessageAsync(string channelId, string content, List<string> attachmentIds = null)
        {
            if ((content == "" | content == null) && (attachmentIds == null))
                throw new Exception("Cannot send empty message without an attachment.");
            var req = new RestRequest($"/channels/{channelId}/messages", Method.POST);
            req.AddJsonBody(JsonConvert.SerializeObject(new SendMessageRequest
            {
                Content = content,
                Attachments = attachmentIds
            }));
            var res = await Client._restClient.ExecutePostAsync(req);
            return Client._deserialize<SelfMessage>(res.Content);
        }

        public Task BeginTypingAsync(string channelId)
        {
            return Client._webSocket.SendInstant(JsonConvert.SerializeObject(new
            {
                type = "BeginTyping",
                channel = channelId
            }));
        }

        public Task EndTypingAsync(string channelId)
        {
            return Client._webSocket.SendInstant(JsonConvert.SerializeObject(new
            {
                type = "EndTyping",
                channel = channelId
            }));
        }

        /// <summary>
        /// Closes DM channel or leaves group channel
        /// </summary>
        /// <param name="channelId"></param>
        /// <returns></returns>
        public Task LeaveAsync(string channelId)
            => Client._restClient.ExecuteAsync(new RestRequest($"/channels/{channelId}", Method.DELETE));

        public Task AddGroupMemberAsync(string channelId, string userId)
            => Client._restClient.ExecuteAsync(new RestRequest($"/channels/{channelId}/recipients/{userId}",
                Method.PUT));

        public Task RemoveGroupMemberAsync(string channelId, string userId)
            => Client._restClient.ExecuteAsync(new RestRequest($"/channels/{channelId}/recipients/{userId}",
                Method.DELETE));

        public Task EditMessageAsync(string channelId, string messageId, string newContent)
        {
            var req = new RestRequest($"/channels/{channelId}/messages/{messageId}");
            req.AddJsonBody(new
            {
                content = newContent
            });
            return Client._restClient.ExecuteAsync(req, Method.PATCH);
        }

        public Task DeleteMessageAsync(string channelId, string id)
            => Client._restClient.ExecuteAsync(new RestRequest($"/channels/{channelId}/messages/{id}"), Method.DELETE);
        
        public async Task<Message[]> GetMessagesAsync(string channelId, int limit, string before = null, string after = null,
            MessageSort sort = MessageSort.Latest)
        {
            var req = new RestRequest($"/channels/{channelId}/messages");
            req.AddJsonBody(JsonConvert.SerializeObject(new GetMessagesRequest()
            {
                limit = limit,
                after = after!,
                before = before!,
                sort = sort.ToString()
            }));
            var res = await Client._restClient.ExecuteGetAsync(req);
            return JsonConvert.DeserializeObject<Message[]>(res.Content)!;
        }
        
        private struct GetMessagesRequest
        {
            public int limit;
            public string before;
            public string after;
            public string sort;
        }
    }

    public enum MessageSort
    {
        Latest,
        Oldest
    }
}