using System;
using System.Threading.Tasks;
using Anargy;
using Anargy.Attributes;
using Anargy.Info;
using Anargy.Results;
using Anargy.Revolt;
using Revolt;
using Taco.CommandHandling;

namespace Taco.Attributes
{
    // todo: make it fetch self owner and move to Anargy.Revolt
    public class RequireBotOwnerAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var revContext = (RevoltCommandContext)context;
            if (revContext.Message.AuthorId == Program.BotOwnerId)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("This command can only be executed by the developer of this bot."));
        }
    }
}