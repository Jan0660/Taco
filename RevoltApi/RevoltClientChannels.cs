﻿using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using RevoltApi.Channels;

namespace RevoltApi
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
        
        public async Task<Message> SendMessageAsync(string channelId, string content, string attachmentId = null)
        {
            var req = new RestRequest($"/channels/{channelId}/messages", Method.POST);
            req.AddJsonBody(JsonConvert.SerializeObject(new SendMessageRequest
            {
                Content = content,
                AttachmentId = attachmentId
            }));
            var res = await Client._restClient.ExecutePostAsync(req);
            return Client._deserialize<Message>(res.Content);
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
    }
}