using System;
using System.Threading.Tasks;
using Revolt.Channels;
using Revolt.Commands.Info;
using Revolt.Commands.Results;

namespace Revolt.Commands.Attributes.Preconditions
{
    public class RequireGroupOwnerAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            if (context is RevoltCommandContext { Channel: GroupChannel groupChannel } revContext &&
                groupChannel.OwnerId == revContext.Message.AuthorId)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(
                PreconditionResult.FromError("This command can only be ran by the owner of this group."));
        }
    }
}