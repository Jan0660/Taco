using System;
using System.Collections.Generic;
using System.Linq;
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


        /// <summary>
        /// Gets a channel from cache by id, if not cached, fetches it.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Channel Get(string id)
        {
            Channel channel = Client.ChannelsCache.FirstOrDefault(u => u._id == id);
            if (channel != null)
                return channel;
            channel = Client._deserializeChannel(Client._restClient.Get(new RestRequest($"/channels/{id}/")).Content);
            Client.CacheChannel(channel);
            return channel;
        }

        public Channel? GetCached(string id)
            => Client.ChannelsCache.FirstOrDefault(c => c._id == id);

        public Task<SelfMessage> SendMessageAsync(string channelId, string content,
            List<string>? attachmentIds = null, MessageReply[]? replies = null, MessageMasquerade? mask = null)
        {
            if ((content == "" | content == null) && (attachmentIds == null))
                throw new Exception("Cannot send empty message without an attachment.");
            return Client._requestAsync<SelfMessage>($"/channels/{channelId}/messages", Method.POST,
                JsonConvert.SerializeObject(new SendMessageRequest
                {
                    Content = content,
                    Attachments = attachmentIds,
                    Replies = replies,
                    Mask = mask,
                }));
        }

        public Task BeginTypingAsync(string channelId)
            => Client._webSocket!.SendInstant(JsonConvert.SerializeObject(new
            {
                type = "BeginTyping",
                channel = channelId
            }));

        public Task EndTypingAsync(string channelId)
            => Client._webSocket!.SendInstant(JsonConvert.SerializeObject(new
            {
                type = "EndTyping",
                channel = channelId
            }));

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
            => Client._requestAsync($"{Client.ApiUrl}/channels/{channelId}/messages/{messageId}", Method.PATCH, JsonConvert.SerializeObject(new
            {
                content = newContent
            }));

        public Task DeleteMessageAsync(string channelId, string id)
            => Client._requestAsync($"{Client.ApiUrl}/channels/{channelId}/messages/{id}", Method.DELETE);

        public async Task<Message[]> GetMessagesAsync(string channelId, int limit, string? before = null,
            string? after = null,
            MessageSort sort = MessageSort.Latest)
        {
            var req = new RestRequest($"/channels/{channelId}/messages");
            req.AddParameter("limit", limit);
            if (after != null)
                req.AddParameter("after", after!);
            if (before != null)
                req.AddParameter("before", before!);
            req.AddParameter("sort", sort.ToString());
            var res = await Client._restClient.ExecuteGetAsync(req);
            var messages = JsonConvert.DeserializeObject<Message[]>(res.Content)!;
            foreach (var msg in messages)
                msg.Client = Client;
            return messages;
        }

        public async Task<User[]> GetMembersAsync(string id)
            => (await Client._requestAsync<User[]>($"/channels/{id}/members")).AttachClient(Client);
    }

    public enum MessageSort
    {
        Latest,
        Oldest
    }
}