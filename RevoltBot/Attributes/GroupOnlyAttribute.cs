using System.Threading.Tasks;
using RevoltApi;
using RevoltApi.Channels;

namespace RevoltBot.Attributes
{
    public class GroupOnlyAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> Evaluate(Message message)
        {
            if (message.Channel is GroupChannel)
                return PreconditionResult.FromSuccess();
            return PreconditionResult.FromError("This command can only be executed in a group channel.");
        }
    }
}