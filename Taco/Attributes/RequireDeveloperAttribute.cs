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
    public class RequireDeveloperAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var revContext = (RevoltCommandContext)context;
            var data = Mongo.GetUserData(revContext.Message.AuthorId);
            if (data is {PermissionLevel: PermissionLevel.Developer})
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError(""));
        }
    }
}