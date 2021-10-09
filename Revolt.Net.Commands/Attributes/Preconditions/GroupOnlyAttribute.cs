using System;
using System.Threading.Tasks;
using Revolt.Channels;
using Revolt.Commands.Info;
using Revolt.Commands.Results;

namespace Revolt.Commands.Attributes.Preconditions
{
    public class GroupOnlyAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var revContext = (RevoltCommandContext)context;
            if (revContext.Channel is GroupChannel)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("This command can only be executed in a group channel."));
        }
    }
}