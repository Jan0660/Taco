using System.Threading.Tasks;
using RevoltApi;
using RevoltApi.Channels;

namespace RevoltBot.Attributes
{
    public class GroupOnlyAttribute : BarePreconditionAttribute
    {
        public override async Task<bool> Evaluate(Message message)
        {
            if (message.Channel is GroupChannel)
                return true;
            await message.Channel.SendMessageAsync("This command can only be executed in a group channel.");
            return false;
        }
    }
}