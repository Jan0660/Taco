using System;
using System.Threading.Tasks;
using Anargy.Attributes;
using Anargy.Info;
using Anargy.Results;
using Revolt.Channels;

namespace Anargy.Revolt.Preconditions
{
    public class TextChannelOnlyAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var revContext = (RevoltCommandContext)context;
            if (revContext.Channel is TextChannel)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("This command can only be executed in a server."));
        }
    }
}