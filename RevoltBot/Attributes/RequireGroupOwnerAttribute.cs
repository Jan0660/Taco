using System.Threading.Tasks;
using RevoltApi;
using RevoltApi.Channels;

namespace RevoltBot.Attributes
{
    public class RequireGroupOwnerAttribute : BarePreconditionAttribute
    {
        public override async Task<bool> Evaluate(Message message)
        {
            if (message.Channel is GroupChannel group && group.OwnerId == message.AuthorId)
                return true;
            await message.Channel.SendMessageAsync("This command can only be ran by the owner of this group.");
            return false;
        }
    }
}