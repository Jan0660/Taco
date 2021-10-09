using System;
using System.Threading.Tasks;
using Revolt.Commands.Info;
using Revolt.Commands.Results;

namespace Revolt.Commands.Attributes.Preconditions
{
    public class RequireBotOwnerAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var revContext = (RevoltCommandContext)context;
            if (revContext.Message.AuthorId == revContext.Client.User.Bot!.OwnerId)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("This command can only be executed by the owner of this bot."));
        }
    }
}