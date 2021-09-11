using System;
using System.Threading.Tasks;
using Anargy.Attributes;
using Anargy.Info;
using Anargy.Results;
using Revolt.Channels;

namespace Anargy.Revolt.Preconditions
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