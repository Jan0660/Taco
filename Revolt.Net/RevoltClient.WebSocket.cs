using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Revolt.Channels;
using Revolt.Internal;

#pragma warning disable 4014
namespace Revolt
{
    public partial class RevoltClient
    {
        /// <summary>
        /// The WebSocket ping in milliseconds, refreshed around every 30 seconds
        /// </summary>
        public long WebSocketPing { get; private set; }

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
                        case "Pong":
                        {
                            var pongTime = packet.Value<long>("data");
                            var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            WebSocketPing = time - pongTime;
                            _onWebSocketPong.InvokeAsync();
                            break;
                        }
                        case "Ready":
                        {
                            State = RevoltClientState.WebSocketReady;
                            _pingTimer?.Stop();
                            _pingTimer = new Timer(30_000d);
                            _pingTimer.Elapsed += (_, _) =>
                            {
                                var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                _webSocket.Send(JsonConvert.SerializeObject(new
                                {
                                    type = "Ping",
                                    data = time
                                }));
                            };
                            _pingTimer.Start();
                            // initialize cache
                            _users = new();
                            _channels = new();
                            _servers = new();
                            foreach (var userToken in packet["users"]!)
                            {
                                var user = userToken.ToObject<User>();
                                user!.AttachClient(this);
                                _users.TryAdd(user._id, user);
                            }

                            foreach (var channelToken in packet["channels"]!)
                            {
                                var channel = _deserializeChannel((JObject)channelToken);
                                if (channel is MessageChannel { LastMessage: { } } messageChannel)
                                    messageChannel.LastMessage.Client = this;
                                _channels.TryAdd(channel._id, channel);
                            }

                            foreach (var serverToken in packet["servers"]!)
                            {
                                var server = serverToken.ToObject<Server>();
                                server!.Client = this;
                                _servers.TryAdd(server._id, server);
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
                            if (_users.TryGetValue(id, out var user))
                            {
                                user.Relationship = status;
                            }

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
                            // todo: MustAdd
                            _channels.TryAdd(channel._id, channel);
                            // todo: TextChannel specific handling to add to Server
                            _channelCreate.InvokeAsync(channel);

                            break;
                        }
                        case "ChannelUpdate":
                        {
                            var id = packet.Value<string>("id");
                            var channel = _channels[id];
                            var data = packet.Value<JObject>("data");
                            if (channel is GroupChannel groupChannel)
                                if (data!.TryGetValue("icon", out var icon))
                                    groupChannel.Icon = icon.ToObject<Attachment>()!;
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
                            {
                                user.Avatar = data.Value<JObject>("avatar")!.ToObject<Attachment>()!;
                                user.Avatar!.Client = this;
                            }

                            if (data.ContainsKey("username"))
                                user.Username = data.Value<string>("username")!;

                            break;
                        }
                        case "ServerDelete":
                        {
                            var id = packet.Value<string>("id");
                            var server = _servers[id];
                            if (server != null)
                                _servers.TryRemove(server._id, out _);
                            // todo: if not cached only id
                            _serverDeleted.InvokeAsync(server);
                            break;
                        }
                        case "ServerMemberUpdate":
                        {
                            var ids = packet.Value<JObject>("id");
                            var serverId = ids.Value<string>("server");
                            var userId = ids.Value<string>("user");
                            var server = _servers[serverId];
                            var memberIndex = server.MemberCache.FindIndex(m => m._id.User == userId);
                            Member cached = null;
                            if (memberIndex != -1)
                                cached = server.MemberCache[memberIndex];
                            var partialMember = packet.SelectToken("data")?.ToObject<Member>();
                            if (partialMember != null)
                            {
                                cached.FillPartial(partialMember, packet.Value<string>("clear"));
                                if (memberIndex != -1)
                                    server.MemberCache[memberIndex] = partialMember;
                                else
                                    server.MemberCache.Add(partialMember);
                            }

                            _serverMemberUpdated.InvokeAsync(cached, partialMember);
                            break;
                        }
                        case "ServerMemberJoin":
                        {
                            _serverMemberJoin.InvokeAsync(packet.Value<string>("id"), packet.Value<string>("user"));
                            break;
                        }
                        case "ServerMemberLeave":
                        {
                            _serverMemberLeave.InvokeAsync(packet.Value<string>("id"), packet.Value<string>("user"));
                            break;
                        }
                        case "ServerRoleDelete":
                        {
                            var serverId = packet.Value<string>("id");
                            var roleId = packet.Value<string>("role_id");
                            var server = _servers[serverId];
                            if (server != null)
                            {
                                var role = server.Roles[roleId!];
                                server.Roles.Remove(roleId);
                                _serverRoleDeleted.InvokeAsync(server, role);
                            }

                            break;
                        }
                        case "ServerRoleUpdate":
                        {
                            var serverId = packet.Value<string>("id");
                            var roleId = packet.Value<string>("role_id");
                            var server = _servers[serverId];
                            if (server != null)
                            {
                                // null if role has just been created
                                server.Roles ??= new();
                                var role = server.Roles.TryGetValue(roleId!, out var oldRole) ? oldRole : null;
                                var partial = packet.Value<JObject>("data")?.ToObject<Role>();
                                if (role != null)
                                    role.FillPartial(partial!, packet.Value<string>("clear"));
                                server.Roles[roleId] = partial;
                                _serverRoleUpdated.InvokeAsync(server, roleId, role, partial);
                            }

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