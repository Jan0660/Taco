using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using Revolt.Channels;
using Websocket.Client;
using Console = Log73.Console;

namespace Revolt
{
    public class RevoltClient
    {
        internal RestClient _restClient = new("https://api.revolt.chat/");
        public RevoltApiInfo ApiInfo { get; }
        internal WebsocketClient _webSocket { get; set; }
        private Session _session { get; }
        private List<User> _users = new();
        public IReadOnlyList<User> UsersCache => _users.AsReadOnly();
        private List<Channel> _channels = new();
        public IReadOnlyList<Channel> ChannelsCache => _channels.AsReadOnly();
        private Timer _pingTimer;
        public string ApiUrl { get; set; } = "https://api.revolt.chat";
        public string AutumnUrl { get; set; } = "https://autumn.revolt.chat";
        public string VortexUrl { get; set; } = "https://voso.revolt.chat";

        #region events

        private List<Func<Message, Task>> _messageReceived = new();

        public event Func<Message, Task> MessageReceived
        {
            add => _messageReceived.Add(value);
            remove => _messageReceived.Remove(value);
        }

        private List<Func<ObjectMessage, Task>> _systemMessageReceived = new();

        public event Func<ObjectMessage, Task> SystemMessageReceived
        {
            add => _systemMessageReceived.Add(value);
            remove => _systemMessageReceived.Remove(value);
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

        private List<Func<Channel, Task>> _channelCreate = new();

        public event Func<Channel, Task> ChannelCreate
        {
            add => _channelCreate.Add(value);
            remove => _channelCreate.Remove(value);
        }

        // todo: ChannelUpdate
        private List<Func<string, string, Task>> _channelGroupJoin = new();

        /// <summary>
        /// GroupId, UserId
        /// </summary>
        public event Func<string, string, Task> ChannelGroupJoin
        {
            add => _channelGroupJoin.Add(value);
            remove => _channelGroupJoin.Remove(value);
        }

        private List<Func<string, string, Task>> _channelGroupLeave = new();

        /// <summary>
        /// GroupId, UserId
        /// </summary>
        public event Func<string, string, Task> ChannelGroupLeave
        {
            add => _channelGroupLeave.Add(value);
            remove => _channelGroupLeave.Remove(value);
        }

        private List<Func<string, Task>> _channelDelete = new();

        public event Func<string, Task> ChannelDelete
        {
            add => _channelDelete.Add(value);
            remove => _channelDelete.Remove(value);
        }

        private List<Func<string, bool, Task>> _userPresence = new();

        /// <summary>
        /// UserId, Online
        /// </summary>
        public event Func<string, bool, Task> UserPresence
        {
            add => _userPresence.Add(value);
            remove => _userPresence.Remove(value);
        }

        private List<Func<string, JObject, Task>> _channelUpdate = new();

        public event Func<string, JObject, Task> ChannelUpdate
        {
            add => _channelUpdate.Add(value);
            remove => _channelUpdate.Remove(value);
        }

        #endregion

        public RevoltClientChannels Channels { get; private set; }
        public RevoltClientUsers Users { get; private set; }
        public RevoltClientSelf Self { get; private set; }

        public RevoltClient(Session session)
        {
            _session = session;
            _restClient.AddDefaultHeader("x-user-id", session.UserId);
            _restClient.AddDefaultHeader("x-session-token", session.SessionToken);
            this.ApiInfo = GetApiInfo().Result;
            _webSocket = new(new Uri(ApiInfo.WebsocketUrl));
            this.Channels = new RevoltClientChannels(this);
            this.Users = new RevoltClientUsers(this);
            this.Self = new RevoltClientSelf(this);
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
                            try
                            {
                                var msg = _deserialize<Message>(message.Text);
                                foreach (var handler in _messageReceived)
                                {
                                    handler.Invoke(msg);
                                }
                            }
                            catch
                            {
                                var msg = _deserialize<ObjectMessage>(message.Text);
                                foreach (var handler in _systemMessageReceived)
                                {
                                    handler.Invoke(msg);
                                }
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
                                // initialize cache
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
                                    if (channel is MessageChannel {LastMessage: { }} messageChannel)
                                        messageChannel.LastMessage.Client = this;
                                    _channels.Add(channel);
                                }

                                foreach (var handler in _onReady)
                                {
                                    handler.Invoke();
                                }
                            }
                            break;
                        }
                        case "UserPresence":
                        {
                            var userId = packet.Value<string>("id");
                            var online = packet.Value<bool>("online");
                            var user = Users.Get(userId);
                            if (user != null)
                                user.Online = packet.Value<bool>("online");
                            foreach (var handler in _userPresence)
                            {
                                handler.Invoke(userId, online);
                            }

                            break;
                        }
                        case "UserRelationship":
                        {
                            var id = packet.Value<string>("user");
                            // if (id == "01EXAG0ZFX02W7PNQE7W5MT339")
                            //     return;
                            var status = (RelationshipStatus) Enum.Parse(typeof(RelationshipStatus),
                                packet.Value<string>("status")!);
                            // update user if they're in cache
                            var user = _users.FirstOrDefault(u => u._id == id);
                            if (user != null)
                                user.Relationship = status;

                            foreach (var handler in _userRelationshipUpdated)
                            {
                                handler.Invoke(id, status);
                            }

                            break;
                        }
                        case "MessageUpdate":
                        {
                            var messageId = packet.Value<string>("id");
                            MessageEditData data = packet.Value<JObject>("data").ToObject<MessageEditData>();

                            foreach (var handler in _messageUpdated)
                            {
                                handler.Invoke(messageId, data);
                            }

                            break;
                        }
                        case "ChannelGroupJoin":
                        {
                            var groupId = packet.Value<string>("id");
                            var userId = packet.Value<string>("user");
                            foreach (var handler in _channelGroupJoin)
                            {
                                handler.Invoke(groupId, userId);
                            }

                            break;
                        }
                        case "ChannelGroupLeave":
                        {
                            var groupId = packet.Value<string>("id");
                            var userId = packet.Value<string>("user");
                            foreach (var handler in _channelGroupLeave)
                            {
                                handler.Invoke(groupId, userId);
                            }

                            break;
                        }
                        case "ChannelDelete":
                            var channelId = packet.Value<string>("id");
                            foreach (var handler in _channelDelete)
                            {
                                handler.Invoke(channelId);
                            }

                            break;
                        case "ChannelCreate":
                        {
                            var channel = packet.ToObject<Channel>();
                            _channels.Add(channel);
                            foreach (var handler in _channelCreate)
                            {
                                handler.Invoke(channel);
                            }

                            break;
                        }
                        case "ChannelUpdate":
                        {
                            var id = packet.Value<string>("id");
                            var channel = (GroupChannel) _channels.First(c => c._id == id);
                            var data = packet.Value<JObject>("data");
                            if (data!.TryGetValue("icon", out var icon))
                                channel.Icon = icon.ToObject<Attachment>()!;

                            foreach (var handler in _channelUpdate)
                            {
                                handler.Invoke(id, data);
                            }

                            break;
                        }
                        case "UserUpdate":
                        {
                            // todo: handle status changes
                            var id = packet.Value<string>("id");
                            User user = UsersCache.FirstOrDefault(u => u._id == id);
                            if (user == null)
                                return;
                            JObject data = packet.Value<JObject>("data");
                            if (data.ContainsKey("avatar"))
                                user.Avatar = data.Value<JObject>("avatar")!.ToObject<Attachment>()!;

                            if (data.ContainsKey("username"))
                                user.Username = data.Value<string>("username")!;

                            break;
                        }
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

        public async Task<string> UploadFile(string name, string path, string tag = "attachments")
        {
            var aut = new RestClient(AutumnUrl);
            var req = new RestRequest($"/{tag}");
            req.AddFile(name, path);
            var res = await aut.ExecutePostAsync(req);
            var obj = JObject.Parse(res.Content);
            return obj.Value<string>("id");
        }

        public async Task<string> UploadFile(string name, byte[] data, string tag = "attachments")
        {
            var aut = new RestClient(AutumnUrl);
            var req = new RestRequest($"/{tag}");
            req.AddFile(name, data, name);
            var res = await aut.ExecutePostAsync(req);
            var obj = JObject.Parse(res.Content);
            return obj.Value<string>("id");
        }

        public Task UpdateAvatarId(string id)
        {
            var req = new RestRequest("/users/id");
            req.AddJsonBody(JsonConvert.SerializeObject(new
            {
                avatar = id
            }));
            return _restClient.ExecuteAsync(req);
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

        public async Task<VortexInformation> GetVortexInfo()
        {
            return JsonConvert.DeserializeObject<VortexInformation>(
                (await (new RestClient(VortexUrl).ExecuteGetAsync(new RestRequest(VortexUrl)))).Content)!;
        }

        public async Task<AutumnInformation> GetAutumnInfo()
        {
            return JsonConvert.DeserializeObject<AutumnInformation>(
                (await (new RestClient(VortexUrl).ExecuteGetAsync(new RestRequest(AutumnUrl)))).Content)!;
        }
    }

    public class SendMessageRequest
    {
        private static Random rng = new();
        [JsonProperty("content")] public string Content;
        [JsonProperty("nonce")] public string Nonce = DateTimeOffset.Now.ToUnixTimeSeconds() + rng.Next().ToString();
        [JsonProperty("attachment")] public string AttachmentId = null;
    }
}