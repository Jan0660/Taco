using System.Threading.Tasks;
using Revolt;
using Revolt.Channels;
using Taco.CommandHandling;

namespace Taco.Attributes
{
    public class GroupOnlyAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> Evaluate(CommandContext context)
        {
            if (context.Channel is GroupChannel)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("This command can only be executed in a group channel."));
        }
    }
}