using System.Threading.Tasks;
using RestSharp;

namespace RevoltApi
{
    public class SelfMessage : Message
    {
        public Task EditAsync(string newContent)
            => Client.Channels.EditMessageAsync(ChannelId, _id, newContent);

        public Task DeleteAsync()
            => Client.Channels.DeleteMessageAsync(ChannelId, _id);
    }
}