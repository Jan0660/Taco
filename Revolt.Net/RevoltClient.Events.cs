using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Revolt.Channels;
using Revolt.Internal;
using Websocket.Client;
using Websocket.Client.Models;

namespace Revolt
{
    public partial class RevoltClient
    {
        private AsyncEvent<Func<Message, Task>> _messageReceived = new();

        public event Func<Message, Task> MessageReceived
        {
            add => _messageReceived.Add(value);
            remove => _messageReceived.Remove(value);
        }

        private AsyncEvent<Func<ObjectMessage, Task>> _systemMessageReceived = new();

        public event Func<ObjectMessage, Task> SystemMessageReceived
        {
            add => _systemMessageReceived.Add(value);
            remove => _systemMessageReceived.Remove(value);
        }

        private AsyncEvent<Func<Task>> _onReady = new();

        public event Func<Task> OnReady
        {
            add => _onReady.Add(value);
            remove => _onReady.Remove(value);
        }
        
        private AsyncEvent<Func<Task>> _onWebSocketPong = new();
        
        /// <summary>
        /// When WebSocket ping is updated, around every 30 seconds.
        /// </summary>
        public event Func<Task> OnWebSocketPong
        {
            add => _onWebSocketPong.Add(value);
            remove => _onWebSocketPong.Remove(value);
        }

        private AsyncEvent<Func<string, Task>> _messageDeleted = new();

        public event Func<string, Task> MessageDeleted
        {
            add => _messageDeleted.Add(value);
            remove => _messageDeleted.Remove(value);
        }

        private AsyncEvent<Func<string, RelationshipStatus, Task>> _userRelationshipUpdated = new();

        public event Func<string, RelationshipStatus, Task> UserRelationshipUpdated
        {
            add => _userRelationshipUpdated.Add(value);
            remove => _userRelationshipUpdated.Remove(value);
        }

        private AsyncEvent<Func<string, MessageEditData, Task>> _messageUpdated = new();

        public event Func<string, MessageEditData, Task> MessageUpdated
        {
            add => _messageUpdated.Add(value);
            remove => _messageUpdated.Remove(value);
        }

        private AsyncEvent<Func<string, JObject, ResponseMessage, Task>> _packetReceived = new();

        /// <summary>
        /// Raised before a packet is handled. Ran asynchronously/not awaited.
        /// </summary>
        public event Func<string, JObject, ResponseMessage, Task> PacketReceived
        {
            add => _packetReceived.Add(value);
            remove => _packetReceived.Remove(value);
        }

        private AsyncEvent<Func<string?, JObject?, ResponseMessage, Exception, Task>> _packetError = new();

        /// <summary>
        /// Invoked when an exception occurs when handling a websocket packet.
        /// </summary>
        public event Func<string?, JObject?, ResponseMessage, Exception, Task> PacketError
        {
            add => _packetError.Add(value);
            remove => _packetError.Remove(value);
        }

        private AsyncEvent<Func<Channel, Task>> _channelCreate = new();

        public event Func<Channel, Task> ChannelCreate
        {
            add => _channelCreate.Add(value);
            remove => _channelCreate.Remove(value);
        }

        // todo: ChannelUpdate
        private AsyncEvent<Func<string, string, Task>> _channelGroupJoin = new();

        /// <summary>
        /// GroupId, UserId
        /// </summary>
        public event Func<string, string, Task> ChannelGroupJoin
        {
            add => _channelGroupJoin.Add(value);
            remove => _channelGroupJoin.Remove(value);
        }

        private AsyncEvent<Func<string, string, Task>> _channelGroupLeave = new();

        /// <summary>
        /// GroupId, UserId
        /// </summary>
        public event Func<string, string, Task> ChannelGroupLeave
        {
            add => _channelGroupLeave.Add(value);
            remove => _channelGroupLeave.Remove(value);
        }

        private AsyncEvent<Func<string, Task>> _channelDelete = new();

        public event Func<string, Task> ChannelDelete
        {
            add => _channelDelete.Add(value);
            remove => _channelDelete.Remove(value);
        }

        private AsyncEvent<Func<string, bool, Task>> _userPresence = new();

        /// <summary>
        /// UserId, Online
        /// </summary>
        public event Func<string, bool, Task> UserPresence
        {
            add => _userPresence.Add(value);
            remove => _userPresence.Remove(value);
        }

        private AsyncEvent<Func<string, JObject, Task>> _channelUpdate = new();

        public event Func<string, JObject, Task> ChannelUpdate
        {
            add => _channelUpdate.Add(value);
            remove => _channelUpdate.Remove(value);
        }

        private AsyncEvent<Func<DisconnectionInfo, Task>> _disconnected = new();

        public event Func<DisconnectionInfo, Task> Disconnected
        {
            add => _disconnected.Add(value);
            remove => _disconnected.Remove(value);
        }

        private AsyncEvent<Func<ReconnectionInfo, Task>> _reconnected = new();

        public event Func<ReconnectionInfo, Task> Reconnected
        {
            add => _reconnected.Add(value);
            remove => _reconnected.Remove(value);
        }

        private AsyncEvent<Func<Server, Task>> _serverDeleted = new();

        public event Func<Server, Task> ServerDeleted
        {
            add => _serverDeleted.Add(value);
            remove => _serverDeleted.Remove(value);
        }

        private AsyncEvent<Func<Member, Member, Task>> _serverMemberUpdated = new();

        public event Func<Member, Member, Task> ServerMemberUpdated
        {
            add => _serverMemberUpdated.Add(value);
            remove => _serverMemberUpdated.Remove(value);
        }

        private AsyncEvent<Func<string, string, Task>> _serverMemberLeave = new();

        public event Func<string, string, Task> ServerMemberLeave
        {
            add => _serverMemberLeave.Add(value);
            remove => _serverMemberLeave.Remove(value);
        }

        private AsyncEvent<Func<string, string, Task>> _serverMemberJoin = new();

        public event Func<string, string, Task> ServerMemberJoin
        {
            add => _serverMemberJoin.Add(value);
            remove => _serverMemberJoin.Remove(value);
        }

        private AsyncEvent<Func<Server, Role, Task>> _serverRoleDeleted = new();

        public event Func<Server, Role, Task> ServerRoleDeleted
        {
            add => _serverRoleDeleted.Add(value);
            remove => _serverRoleDeleted.Remove(value);
        }

        private AsyncEvent<Func<Server, string, Role, Role, Task>> _serverRoleUpdated = new();

        /// <summary>
        /// Server, Role Id, Old Role, New Role
        /// </summary>
        public event Func<Server, string, Role, Role, Task> ServerRoleUpdated
        {
            add => _serverRoleUpdated.Add(value);
            remove => _serverRoleUpdated.Remove(value);
        }
    }
}