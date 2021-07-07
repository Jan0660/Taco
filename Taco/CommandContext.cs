using Revolt;
using Revolt.Channels;

namespace Taco
{
    public class CommandContext
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

        public Message Message { get; init; }

        public User User => Message.Author;

        public CommandContext(Message message)
        {
            Message = message;
        }

        public UserData GetUserData()
        {
            var data = Mongo.GetUserData(Message.AuthorId);
            if (data != null)
                _cachedUserData = data;
            return data;
        }
    }
}