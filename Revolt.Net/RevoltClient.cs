using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using Revolt.Channels;
using Websocket.Client;
using Websocket.Client.Models;

namespace Revolt
{
    public class RevoltClient
    {
        internal RestClient _restClient = new("https://api.revolt.chat/");
        public RevoltApiInfo? ApiInfo { get; private set; }
        internal WebsocketClient? _webSocket { get; private set; }
        internal Session? _session { get; set; }
        internal List<User> _users = new();
        public IReadOnlyList<User> UsersCache => _users.AsReadOnly();
        private List<Channel> _channels = new();
        public IReadOnlyList<Channel> ChannelsCache => _channels.AsReadOnly();

        public List<Server> ServersCache { get; internal set; }
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
        private List<Func<DisconnectionInfo, Task>> _disconnected = new();

        public event Func<DisconnectionInfo, Task> Disconnected
        {
            add => _disconnected.Add(value);
            remove => _disconnected.Remove(value);
        }
        private List<Func<ReconnectionInfo, Task>> _reconnected = new();

        public event Func<ReconnectionInfo, Task> Reconnected
        {
            add => _reconnected.Add(value);
            remove => _reconnected.Remove(value);
        }
        
        private List<Func<Server, Task>> _serverDeleted = new();

        public event Func<Server, Task> ServerDeleted
        {
            add => _serverDeleted.Add(value);
            remove => _serverDeleted.Remove(value);
        }
        private List<Func<Member, Member, Task>> _serverMemberUpdated = new();

        public event Func<Member, Member, Task> ServerMemberUpdated
        {
            add => _serverMemberUpdated.Add(value);
            remove => _serverMemberUpdated.Remove(value);
        }

        #endregion

        public RevoltClientChannels Channels { get; private set; }
        public RevoltClientUsers Users { get; private set; }
        public RevoltClientSelf Self { get; private set; }
        public RevoltClientServers Servers { get; private set; }

        /// <summary>
        /// Create an unauthenticated client, use another constructor overload or <see cref="LoginAsync"/> for authenticating.
        /// </summary>
        public RevoltClient()
        {
            this.Channels = new RevoltClientChannels(this);
            this.Users = new RevoltClientUsers(this);
            this.Self = new RevoltClientSelf(this);
            this.Servers = new RevoltClientServers(this);
        }

        public RevoltClient(Session session) : this()
        {
            _useSession(session);
        }

        public RevoltClient(string botToken, string userId) : this()
        {
            _restClient.AddDefaultHeader("x-bot-token", botToken);
            _botToken = botToken;
            _authType = AuthType.Bot;
            Self.UserId = userId;
        }

        private void _useSession(Session session)
        {
            _session = session;
            _restClient.AddDefaultHeader("x-user-id", session.UserId);
            _restClient.AddDefaultHeader("x-session-token", session.SessionToken);
            _authType = AuthType.User;
            Self.UserId = _session.UserId;
        }

        /// <summary>
        /// Creates a session and automatically uses it.
        /// </summary>
        /// <returns>The session that was created.</returns>
        public async Task<Session> LoginAsync(string email, string password, string? deviceName = null,
            string? captcha = null)
        {
            var session = await _requestAsync<Session>($"{ApiUrl}/auth/login", body: JsonConvert.SerializeObject(new
            {
                email, password, device_name = deviceName, captcha
            }));
            _useSession(session);
            return session;
        }

        /// <summary>
        /// Connects the client to the websocket.
        /// </summary>
        public async Task ConnectWebSocketAsync()
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (this.ApiInfo == null || this._webSocket == null)
            {
                this.ApiInfo = GetApiInfo();
                _webSocket = new(new Uri(ApiInfo.WebsocketUrl));
            }

            // todo: add disconnect and reconnect events
            _webSocket.ReconnectTimeout = null;
            _webSocket.DisconnectionHappened.Subscribe(info =>
            {
                // is null if disconnected using DisconnectWebSocket()
                if (_webSocket != null)
                    _webSocket.Start();
                foreach (var handler in _disconnected)
                {
                    handler.Invoke(info);
                }
            });
            _webSocket.ReconnectionHappened.Subscribe((info =>
            {
                var json = JsonConvert.SerializeObject(_authType switch
                {
                    AuthType.User => new
                    {
                        type = "Authenticate", id = _session.Id,
                        user_id = _session.UserId,
                        session_token = _session.SessionToken
                    },
                    AuthType.Bot => new
                    {
                        type = "Authenticate", token = _botToken
                    },
                    _ => throw new Exception("Invalid AuthType.")
                }
                    );
                _webSocket.Send(json);
                foreach (var handler in _reconnected)
                {
                    handler.Invoke(info);
                }
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
                                _pingTimer = new Timer(30_000d);
                                _pingTimer.Elapsed += (_, _) =>
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
                                ServersCache = new();
                                foreach (var userToken in packet["users"]!)
                                {
                                    var user = userToken.ToObject<User>();
                                    user!.AttachClient(this);
                                    _users.Add(user);
                                }

                                foreach (var channelToken in packet["channels"]!)
                                {
                                    var channel = _deserializeChannel((JObject)channelToken);
                                    if (channel is MessageChannel { LastMessage: { } } messageChannel)
                                        messageChannel.LastMessage.Client = this;
                                    _channels.Add(channel);
                                }

                                foreach (var serverToken in packet["servers"]!)
                                {
                                    var server = serverToken.ToObject<Server>();
                                    server!.Client = this;
                                    ServersCache.Add(server);
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
                            var id = packet.Value<JObject>("user").Value<string>("_id");
                            var status = (RelationshipStatus)Enum.Parse(typeof(RelationshipStatus),
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
                            var channel = (GroupChannel)_channels.First(c => c._id == id);
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
                            if (data!.ContainsKey("avatar"))
                                user.Avatar = data.Value<JObject>("avatar")!.ToObject<Attachment>()!;

                            if (data.ContainsKey("username"))
                                user.Username = data.Value<string>("username")!;

                            break;
                        }
                        case "ServerDelete":
                        {
                            var id = packet.Value<string>("id");
                            var server = ServersCache.FirstOrDefault(s => s._id == id);
                            foreach (var handler in _serverDeleted)
                                handler.Invoke(server);
                            break;
                        }
                        case "ServerMemberUpdate":
                        {
                            var serverId = packet.Value<string>("id");
                            var partialMember = packet.Value<Member>("data");
                            var server = ServersCache.First(s => s._id == serverId);
                            var memberIndex = server.MemberCache.FindIndex(m => m._id == partialMember!._id);
                            Member cached = null;
                            if (memberIndex != -1)
                                cached = server.MemberCache[memberIndex];
                            var remove = packet.Value<string>("remove");
                            // patch member by copying data from (already cached) onto the partial object received
                            if (cached != null)
                            {
                                if (cached.Nickname != null && partialMember!.Nickname != null)
                                    partialMember.Nickname = cached.Nickname;
                                if (cached.Avatar != null && partialMember!.Avatar != null)
                                    partialMember.Avatar = cached.Avatar;
                                if (cached.Roles != null && partialMember!.Roles != null)
                                    partialMember.Roles = cached.Roles;
                            }
                            if (remove == "Nickname")
                                partialMember!.Nickname = null;
                            if (remove == "Avatar")
                                partialMember!.Avatar = null;
                            if (memberIndex != -1)
                                server.MemberCache[memberIndex] = partialMember;
                            foreach (var handler in _serverMemberUpdated)
                                handler.Invoke(cached, partialMember);
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

        public void DisconnectWebsocket()
        {
            var websocket = _webSocket;
            _webSocket = null!;
            websocket.Stop(WebSocketCloseStatus.NormalClosure, "sus");
            websocket.Dispose();
        }

        public RevoltApiInfo GetApiInfo()
            => JsonConvert.DeserializeObject<RevoltApiInfo>(new WebClient().DownloadString(ApiUrl))!;

        public async Task<RevoltApiInfo> GetApiInfoAsync()
            => JsonConvert.DeserializeObject<RevoltApiInfo>(await new HttpClient().GetStringAsync(ApiUrl))!;

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
                case "TextChannel":
                    channel = obj.ToObject<TextChannel>(new JsonSerializer()
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });
                    break;
                default:
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

        internal Task<T> _requestAsync<T>(string url, Method method = Method.GET, string? body = null)
        {
            var req = new RestRequest(url, method);
            if (body != null)
                req.AddJsonBody(body);
            return _requestAsync<T>(req);
        }

        internal Task _requestAsync(string url, Method method = Method.GET, string? body = null)
        {
            var req = new RestRequest(url, method);
            if (body != null)
                req.AddJsonBody(body);
            return _requestAsync(req);
        }

        internal Task _requestAsync(RestRequest request)
            => _restClient.ExecuteAsync(request);
        internal async Task<T> _requestAsync<T>(RestRequest request)
        {
            var res = await _restClient.ExecuteAsync(request);
            T val;
            try
            {
                val = JsonConvert.DeserializeObject<T>(res.Content, new JsonSerializerSettings()
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
            }
            // todo: catch json exception type JsonReaderException
            catch (Exception exception)
            {
                var err = JsonConvert.DeserializeObject<RevoltError>(res.Content);
                if (res.StatusCode == HttpStatusCode.OK)
                    throw new Exception(
                        "Internal exception deserializing JSON response from Revolt, please check your Revolt.Net version.",
                        exception);
                throw new RevoltException(err!, res);
            }

            switch (val)
            {
                case User user:
                    user.AttachClient(this);
                    break;
                case RevoltObject revoltObject:
                    revoltObject.Client = this;
                    break;
                case RevoltObject[] revoltObjects:
                {
                    foreach (var obj in revoltObjects)
                        obj.Client = this;
                    break;
                }
            }

            return val;
        }

        public VortexInformation GetVortexInfo()
            => JsonConvert.DeserializeObject<VortexInformation>(
                new WebClient().DownloadString(VortexUrl))!;

        public AutumnInformation GetAutumnInfo()
            => JsonConvert.DeserializeObject<AutumnInformation>(
                new WebClient().DownloadString(AutumnUrl))!;

        public async Task<VortexInformation> GetVortexInfoAsync()
            => JsonConvert.DeserializeObject<VortexInformation>(
                await new HttpClient().GetStringAsync(VortexUrl))!;

        public async Task<AutumnInformation> GetAutumnInfoAsync()
            => JsonConvert.DeserializeObject<AutumnInformation>(
                await new HttpClient().GetStringAsync(AutumnUrl))!;

        private static Random rng = new();
        private AuthType _authType;
        private readonly string _botToken;

        public static string GenerateNonce()
            => DateTimeOffset.Now.ToUnixTimeSeconds() + rng.Next().ToString();

        internal void CacheChannel(Channel channel)
        {
            var cached = _channels.FirstOrDefault(c => c._id == channel._id);
            if (cached != null)
                _channels.Remove(cached);
            _channels.Add(channel);
        }
    }

    public class SendMessageRequest
    {
        [JsonProperty("content")] public string Content;
        [JsonProperty("nonce")] public string Nonce = RevoltClient.GenerateNonce();
        [JsonProperty("attachments")] public List<string>? Attachments;
        [JsonProperty("replies")] public MessageReply[] Replies;
    }

    public struct MessageReply
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("mention")] public bool Mention { get; set; }
    }

    public class RevoltException : Exception
    {
        public RevoltError Error { get; }

        public RevoltException(RevoltError error, IRestResponse response) : base(
            $"Revolt responded with {(int)response.StatusCode}: {error.Type}.") => (Error) = (error);
    }

    public class RevoltError
    {
        [JsonProperty("type")] public string Type { get; set; }
    }

    internal static class RevoltInternalExtensionMethods
    {
        public static User AttachClient(this User user, RevoltClient client)
        {
            user.Client = client;
            if (user.Avatar != null)
                user.Avatar.Client = client;
            return user;
        }

        public static User[] AttachClient(this User[] users, RevoltClient client)
        {
            foreach (var user in users)
            {
                user.Client = client;
                if (user.Avatar != null)
                    user.Avatar.Client = client;
            }

            return users;
        }
    }

    internal enum AuthType
    {
        Bot,
        User
    }
}