using System.Threading.Tasks;
using RevoltApi;
using RevoltApi.Channels;
using RevoltBot.CommandHandling;

namespace RevoltBot.Attributes
{
    public class GroupOnlyAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> Evaluate(Message message)
        {
            if (message.Channel is GroupChannel)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("This command can only be executed in a group channel."));
        }
    }
}