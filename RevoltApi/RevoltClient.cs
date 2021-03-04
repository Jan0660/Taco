using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RevoltApi.Channels;
using Websocket.Client;
using Channel = RevoltApi.Channels.Channel;
using Console = Log73.Console;

namespace RevoltApi
{
    public class RevoltClient
    {
        private RestClient _restClient = new("https://api.revolt.chat/");
        public string UserId;
        private string _sessionToken;
        public RevoltApiInfo ApiInfo;
        private WebsocketClient _webSocket;
        private Session _session;
        private List<User> _users = new();
        public IReadOnlyList<User> Users => _users.AsReadOnly();
        private List<Channel> _channels = new();
        public IReadOnlyList<Channel> Channels => _channels.AsReadOnly();
        private Timer _pingTimer;
        public string ApiUrl = "https://api.revolt.chat";
        public string AutumnUrl = "https://autumn.revolt.chat";

        #region events

        private List<Func<Message, Task>> _messageReceived = new();

        public event Func<Message, Task> MessageReceived
        {
            add => _messageReceived.Add(value);
            remove => _messageReceived.Remove(value);
        }

        private List<Func<Task>> _onReady = new();

        public event Func<Task> OnReady
        {
            add => _onReady.Add(value);
            remove => _onReady.Remove(value);
        }

        private List<Func<string, Task>> _messageDeleted = new();

        public event Func<string, Task> MessageDeleted
        {
            add => _messageDeleted.Add(value);
            remove => _messageDeleted.Remove(value);
        }

        // todo: MessageUpdate
        // todo: ChannelCreate
        // todo: ChannelUpdate
        // todo: ChannelGroupJoin
        // todo: ChannelGroupLeave
        // todo: ChannelDelete
        // todo: UserRelationship
        // todo: UserPresence

        #endregion

        public RevoltClient(Session session)
        {
            _session = session;
            this.UserId = session.UserId;
            this._sessionToken = session.SessionToken;
            _restClient.AddDefaultHeader("x-user-id", session.UserId);
            _restClient.AddDefaultHeader("x-session-token", session.SessionToken);
            this.ApiInfo = GetApiInfo().Result;
            _webSocket = new(new Uri(ApiInfo.WebsocketUrl));
        }

        /// <summary>
        /// Connects the client to the websocket.
        /// </summary>
        public async Task ConnectWebSocketAsync()
        {
            _webSocket.ReconnectTimeout = null;
            _webSocket.DisconnectionHappened.Subscribe(info =>
            {
                Console.Error($"websocke disonnect {info.Type}");
                _webSocket.Start();
            });
            _webSocket.ReconnectionHappened.Subscribe((info =>
            {
                Console.Debug($"weboscket reconnected {info.Type}");
                var json = JsonConvert.SerializeObject(new
                {
                    type = "Authenticate", id = _session.Id,
                    user_id = _session.UserId,
                    session_token = _session.SessionToken
                });
                _webSocket.Send(json);
            }));

            _webSocket.MessageReceived.Subscribe((message =>
            {
                var packet = JObject.Parse(message.Text);
                Console.Debug($"Message receive: Length: {message.Text.Length}; Type: {packet.Value<string>("type")};");
                switch (packet.Value<string>("type"))
                {
                    case "Message":
                        var msg = _deserialize<Message>(message.Text);
                        foreach (var handler in _messageReceived)
                        {
                            handler.Invoke(msg);
                        }

                        break;
                    case "MessageDelete":
                        var id = packet.Value<string>("id");
                        foreach (var handler in _messageDeleted)
                        {
                            handler.Invoke(id);
                        }

                        break;
                    case "Ready":
                    {
                        _pingTimer?.Stop();
                        _pingTimer = new Timer(1000d);
                        _pingTimer.Elapsed += (sender, args) =>
                        {
                            _webSocket.Send(JsonConvert.SerializeObject(new
                            {
                                type = "Ping",
                                time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                            }));
                        };
                        _pingTimer.Start();
                        _users = new();
                        _channels = new();
                        foreach (var userToken in packet["users"]!)
                        {
                            var user = userToken.ToObject<User>();
                            user!.Client = this;
                            _users.Add(user);
                        }

                        foreach (var channelToken in packet["channels"]!)
                        {
                            var channel = _deserializeChannel((JObject) channelToken);
                            if (channel is MessageChannel messageChannel)
                                messageChannel.LastMessage.Client = this;
                            _channels.Add(channel);
                        }

                        foreach (var handler in _onReady)
                        {
                            handler.Invoke();
                        }

                        break;
                    }
                    case "UserPresence":
                    {
                        var user = GetUser(packet.Value<string>("id"));
                        user.Online = packet.Value<bool>("online");
                        break;
                    }
                }
            }));
            await _webSocket.Start();
        }

        public async Task<RevoltApiInfo> GetApiInfo()
        {
            var res = await _restClient.ExecuteGetAsync(new RestRequest("/"));
            return JsonConvert.DeserializeObject<RevoltApiInfo>(res.Content);
        }

        public async Task<Message> SendMessageToChannel(string channelId, string content, string attachmentId = null)
        {
            var req = new RestRequest($"/channels/{channelId}/messages", Method.POST);
            req.AddJsonBody(JsonConvert.SerializeObject(new SendMessageRequest
            {
                Content = content,
                AttachmentId = attachmentId
            }));
            var res = await _restClient.ExecutePostAsync(req);
            return _deserialize<Message>(res.Content);
        }

        public Channel GetChannel(string id)
        {
            Channel channel = _channels.FirstOrDefault(u => u._id == id);
            if (channel != null)
                return channel;
            return _deserializeChannel(_restClient.Get(new RestRequest($"/channels/{id}/")).Content);
        }

        public User GetUser(string id)
        {
            User user = _users.FirstOrDefault(u => u._id == id);
            if (user != null)
                return user;
            return _getObject<User>($"/users/{id}").Result;
        }

        public async Task<string> UploadFile(string name, string path)
        {
            var aut = new RestClient(AutumnUrl);
            var req = new RestRequest("/");
            req.AddFile(name, path);
            var res = await aut.ExecutePostAsync(req);
            var obj = JObject.Parse(res.Content);
            return obj.Value<string>("id");
        }

        public async Task<string> UploadFile(string name, byte[] data)
        {
            var aut = new RestClient(AutumnUrl);
            var req = new RestRequest("/");
            req.AddFile(name, data, name);
            var res = await aut.ExecutePostAsync(req);
            var obj = JObject.Parse(res.Content);
            return obj.Value<string>("id");
        }

        public async Task BeginTypingAsync(string channelId)
        {
            await _webSocket.SendInstant(JsonConvert.SerializeObject(new
            {
                type = "BeginTyping",
                channel = channelId
            }));
        }

        public async Task EndTypingAsync(string channelId)
        {
            await _webSocket.SendInstant(JsonConvert.SerializeObject(new
            {
                type = "EndTyping",
                channel = channelId
            }));
        }

        private Channel _deserializeChannel(string json)
        {
            var obj = JObject.Parse(json);
            return _deserializeChannel(obj);
        }

        private Channel _deserializeChannel(JObject obj)
        {
            Channel channel;
            switch (obj.Value<string>("channel_type"))
            {
                case "Group":
                    channel = obj.ToObject<GroupChannel>();
                    break;
                case "DirectMessage":
                    channel = obj.ToObject<DirectMessageChannel>();
                    break;
                default:
                    Console.Warn($"Unimplemented channel_type: {obj.Value<string>("channel_type")}");
                    channel = obj.ToObject<Channel>();
                    break;
            }

            channel!.Client = this;
            return channel;
        }

        private T _deserialize<T>(string json) where T : RevoltObject
        {
            T obj = JsonConvert.DeserializeObject<T>(json);
            obj.Client = this;
            return obj;
        }

        private async Task<T> _getObject<T>(string url) where T : RevoltObject
        {
            var req = new RestRequest(url);
            var res = await _restClient.ExecuteGetAsync(req);
            return _deserialize<T>(res.Content);
        }
    }

    public class SendMessageRequest
    {
        [JsonProperty("content")] public string Content;
        [JsonProperty("nonce")] public string Nonce = DateTimeOffset.Now.ToString();
        [JsonProperty("attachment")] public string AttachmentId = null;
    }
}