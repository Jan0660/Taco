using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Revolt.Base;
using Revolt.Channels;

namespace Revolt
{
    public partial class RevoltClient
    {

        /// <summary>
        /// Connects the client to the websocket.
        /// </summary>
        public async Task ConnectWebSocketAsync()
        {
            ThrowIfNotLoggedIn(
                "Log in using the LoginAsync method before trying to initialize a WebSocket connection.");
            if (_webSocket == null)
                _webSocket = new(new Uri(ApiInfo.WebsocketUrl));

            _webSocket.ReconnectTimeout = null;
            _webSocket.DisconnectionHappened.Subscribe(info =>
            {
                State = RevoltClientState.LoggedIn;
                // is null if disconnected using DisconnectWebSocket()
                if (_webSocket != null)
                    _webSocket.Start();
                _disconnected.InvokeAsync(info);
            });
            _webSocket.ReconnectionHappened.Subscribe((info =>
            {
                State = RevoltClientState.WebSocketConnected;
                _webSocket.Send(JsonConvert.SerializeObject(new
                    {
                        type = "Authenticate", token
                    }
                ));
                _reconnected.InvokeAsync(info);
            }));

            _webSocket.MessageReceived.Subscribe((message =>
            {
                JObject packet = null;
                string packetType = null;
                try
                {
                    packet = JObject.Parse(message.Text);
                    packetType = packet.Value<string>("type");
                    _packetReceived.InvokeAsync(packetType, packet, message);
                    switch (packetType)
                    {
                        case "Message":
                            try
                            {
                                var msg = _deserialize<Message>(message.Text);
                                _messageReceived.InvokeAsync(msg);
                            }
                            catch
                            {
                                var msg = _deserialize<ObjectMessage>(message.Text);
                                _systemMessageReceived.InvokeAsync(msg);
                            }

                            break;
                        case "MessageDelete":
                        {
                            var id = packet.Value<string>("id");
                            _messageDeleted.InvokeAsync(id);

                            break;
                        }
                        case "Ready":
                        {
                            State = RevoltClientState.WebSocketReady;
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

                            _onReady.InvokeAsync();
                            break;
                        }
                        case "UserPresence":
                        {
                            var userId = packet.Value<string>("id");
                            var online = packet.Value<bool>("online");
                            var user = Users.Get(userId);
                            if (user != null)
                                user.Online = online;
                            _userPresence.InvokeAsync(userId, online);

                            break;
                        }
                        case "UserRelationship":
                        {
                            var id = packet.Value<JObject>("user")!.Value<string>("_id");
                            var status = (RelationshipStatus)Enum.Parse(typeof(RelationshipStatus),
                                packet.Value<string>("status")!);
                            // update user if they're in cache
                            var user = _users.FirstOrDefault(u => u._id == id);
                            if (user != null)
                                user.Relationship = status;

                            _userRelationshipUpdated.InvokeAsync(id, status);

                            break;
                        }
                        case "MessageUpdate":
                        {
                            var messageId = packet.Value<string>("id");
                            MessageEditData data = packet.Value<JObject>("data").ToObject<MessageEditData>();
                            _messageUpdated.InvokeAsync(messageId, data);

                            break;
                        }
                        case "ChannelGroupJoin":
                        {
                            _channelGroupJoin.InvokeAsync(packet.Value<string>("id"), packet.Value<string>("user"));

                            break;
                        }
                        case "ChannelGroupLeave":
                        {
                            _channelGroupLeave.InvokeAsync(packet.Value<string>("id"), packet.Value<string>("user"));
                            break;
                        }
                        case "ChannelDelete":
                            _channelDelete.InvokeAsync(packet.Value<string>("id"));
                            break;
                        case "ChannelCreate":
                        {
                            var channel = packet.ToObject<Channel>();
                            _channels.Add(channel);
                            // todo: TextChannel specific handling to add to Server
                            _channelCreate.InvokeAsync(channel);

                            break;
                        }
                        case "ChannelUpdate":
                        {
                            var id = packet.Value<string>("id");
                            var channel = (GroupChannel)_channels.First(c => c._id == id);
                            var data = packet.Value<JObject>("data");
                            if (data!.TryGetValue("icon", out var icon))
                                channel.Icon = icon.ToObject<Attachment>()!;
                            _channelUpdate.InvokeAsync(id, data);

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
                            if (server != null)
                                ServersCache.Remove(server);
                            // todo: if not cached only id
                            _serverDeleted.InvokeAsync(server);
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
                            _serverMemberUpdated.InvokeAsync(cached, partialMember);
                            break;
                        }
                    }
                }
                catch (Exception exc)
                {
                    _packetError.InvokeAsync(packetType, packet, message, exc);
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
    }
}