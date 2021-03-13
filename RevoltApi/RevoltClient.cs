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
        internal RestClient _restClient = new("https://api.revolt.chat/");
        public string UserId;
        private string _sessionToken;
        public RevoltApiInfo ApiInfo;
        internal WebsocketClient _webSocket;
        private Session _session;
        private List<User> _users = new();
        public IReadOnlyList<User> UsersCache => _users.AsReadOnly();
        private List<Channel> _channels = new();
        public IReadOnlyList<Channel> ChannelsCache => _channels.AsReadOnly();
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

        private List<Func<string, RelationshipStatus, Task>> _userRelationshipUpdated = new();

        public event Func<string, RelationshipStatus, Task> UserRelationshipUpdated
        {
            add => _userRelationshipUpdated.Add(value);
            remove => _userRelationshipUpdated.Remove(value);
        }

        private List<Func<string, MessageEditData, Task>> _messageUpdated = new();

        public event Func<string, MessageEditData, Task> MessageUpdated
        {
            add => _messageUpdated.Add(value);
            remove => _messageUpdated.Remove(value);
        }
        
        private List<Func<string, JObject, ResponseMessage, Task>> _packetReceived = new();

        /// <summary>
        /// Raised before a packet is handled. Ran asynchronously/not awaited.
        /// </summary>
        public event Func<string, JObject, ResponseMessage, Task> PacketReceived
        {
            add => _packetReceived.Add(value);
            remove => _packetReceived.Remove(value);
        }
        
        private List<Func<string?, JObject?, ResponseMessage, Exception, Task>> _packetError = new();

        /// <summary>
        /// Invoked when an exception occurs when handling a websocket packet.
        /// </summary>
        public event Func<string?, JObject?, ResponseMessage, Exception, Task> PacketError
        {
            add => _packetError.Add(value);
            remove => _packetError.Remove(value);
        }
        // todo: ChannelCreate
        // todo: ChannelUpdate
        // todo: ChannelGroupJoin
        // todo: ChannelGroupLeave
        // todo: ChannelDelete
        // todo: UserPresence

        #endregion

        public RevoltClientChannels Channels { get; private set; }
        public RevoltClientUsers Users { get; private set; }

        public RevoltClient(Session session)
        {
            _session = session;
            this.UserId = session.UserId;
            this._sessionToken = session.SessionToken;
            _restClient.AddDefaultHeader("x-user-id", session.UserId);
            _restClient.AddDefaultHeader("x-session-token", session.SessionToken);
            this.ApiInfo = GetApiInfo().Result;
            _webSocket = new(new Uri(ApiInfo.WebsocketUrl));
            this.Channels = new RevoltClientChannels(this);
            this.Users = new RevoltClientUsers(this);
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
                JObject packet = null;
                string packetType = null;
                try
                {
                    packet = JObject.Parse(message.Text);
                    packetType = packet.Value<string>("type");
                    foreach (var handler in _packetReceived)
                    {
                        handler.Invoke(packetType, packet, message);
                    }

                    switch (packetType)
                    {
                        case "Message":
                            var msg = _deserialize<Message>(message.Text);
                            if (msg.AuthorId == "01EXAG0ZFX02W7PNQE7W5MT339")
                                return;
                            foreach (var handler in _messageReceived)
                            {
                                handler.Invoke(msg);
                            }

                            break;
                        case "MessageDelete":
                        {
                            var id = packet.Value<string>("id");
                            foreach (var handler in _messageDeleted)
                            {
                                handler.Invoke(id);
                            }

                            break;
                        }
                        case "Ready":
                        {
                            _pingTimer?.Stop();
                            _pingTimer = new Timer(30000d);
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
                            var user = Users.Get(packet.Value<string>("id"));
                            user.Online = packet.Value<bool>("online");
                            break;
                        }
                        case "UserRelationship":
                        {
                            var id = packet.Value<string>("user");
                            if (id == "01EXAG0ZFX02W7PNQE7W5MT339")
                                return;
                            var status = Enum.Parse<RelationshipStatus>(packet.Value<string>("status"));
                            var user = _users.FirstOrDefault(u => u._id == id);
                            if (user != null)
                            {
                                user.Relationship = status;
                            }

                            foreach (var handler in _userRelationshipUpdated)
                            {
                                handler.Invoke(id, status);
                            }

                            break;
                        }
                        case "MessageUpdate":
                            var messageId = packet.Value<string>("id");
                            MessageEditData data = packet.Value<JObject>("data").ToObject<MessageEditData>();

                            foreach (var handler in _messageUpdated)
                            {
                                handler.Invoke(messageId, data);
                            }

                            break;
                    }
                }
                catch (Exception exc)
                {
                    foreach (var handler in _packetError)
                    {
                        handler.Invoke(packetType, packet, message, exc);
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


        internal Channel _deserializeChannel(string json)
        {
            var obj = JObject.Parse(json);
            return _deserializeChannel(obj);
        }

        internal Channel _deserializeChannel(JObject obj)
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
                case "SavedMessages":
                    channel = obj.ToObject<SavedMessagesChannel>();
                    break;
                default:
                    Console.Warn($"Unimplemented channel_type: {obj.Value<string>("channel_type")}");
                    channel = obj.ToObject<Channel>();
                    break;
            }

            channel!.Client = this;
            return channel;
        }

        internal T _deserialize<T>(string json) where T : RevoltObject
        {
            T obj = JsonConvert.DeserializeObject<T>(json);
            obj.Client = this;
            return obj;
        }

        internal async Task<T> _getObject<T>(string url) where T : RevoltObject
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