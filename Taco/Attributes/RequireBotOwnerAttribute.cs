using System.Threading.Tasks;
using Revolt;
using Taco.CommandHandling;

namespace Taco.Attributes
{
    public class RequireBotOwnerAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> Evaluate(CommandContext context)
        {
            if (context.Message.AuthorId == Program.BotOwnerId)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError($"Sorry, but this command can only be executed by the developer of this bot, <@{Program.BotOwnerId}>."));
        }
    }
}