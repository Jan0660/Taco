using System.Linq;

namespace RevoltApi
{
    public class RevoltClientUsers
    {
        public RevoltClient Client { get; }
        
        public RevoltClientUsers(RevoltClient client)
        {
            this.Client = client;
        }

        public User Get(string id)
        {
            User user = Client.UsersCache.FirstOrDefault(u => u._id == id);
            if (user != null)
                return user;
            return Client._getObject<User>($"/users/{id}").Result;
        }
    }
}