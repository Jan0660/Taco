using RevoltApi;

namespace RevoltBot
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

        public Message Message { get; init; }

        public User User => Message.Author;

        public CommandContext(Message message)
        {
            Message = message;
        }
    }
}