using System;
using System.Linq;
using System.Threading.Tasks;
using Revolt;
using Revolt.Channels;
using Revolt.Commands;

namespace Taco
{
    public class TacoCommandContext : RevoltCommandContext
    {
        public UserData UserData
        {
            get
            {
                if (_cachedUserData != null)
                    return _cachedUserData;
                _cachedUserData = Mongo.GetOrCreateUserData(Message.AuthorId);
                return _cachedUserData;
            }
        }

        private UserData _cachedUserData;

        public ServerData ServerData
        {
            get
            {
                if (_cachedServerData != null)
                    return _cachedServerData;
                _cachedServerData = Mongo.GetOrCreateServerData(((TextChannel)Message.Channel).ServerId);
                return _cachedServerData;
            }
        }

        private ServerData _cachedServerData;

        public GroupData GroupData
        {
            get
            {
                if (_cachedGroupData != null)
                    return _cachedGroupData;
                if (Message.Channel is not GroupChannel)
                    throw new Exception("Channel is not a GroupChannel.");
                _cachedGroupData = Mongo.GetOrCreateGroupData(Message.ChannelId);
                return _cachedGroupData;
            }
        }

        private GroupData _cachedGroupData;
        private CommunityData _cachedCommunityData;

        public CommunityData CommunityData
        {
            get
            {
                var ret = _cachedServerData ?? _cachedGroupData ?? _cachedCommunityData;
                if (ret != null)
                    return ret;
                if (Message.Channel is GroupChannel)
                {
                    return GroupData;
                }
                return ServerData;
            }
        }

        public TacoCommandContext(Message message) : base(message)
        {
        }

        public UserData GetUserData()
        {
            var data = Mongo.GetUserData(Message.AuthorId);
            if (data != null)
                _cachedUserData = data;
            return data;
        }
        public Task UpdateCommunityDataAsync()
            => ServerData != null ? ServerData.UpdateAsync() : GroupData.UpdateAsync();
    }
}