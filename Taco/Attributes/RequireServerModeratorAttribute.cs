using System;
using System.Linq;
using System.Threading.Tasks;
using Revolt.Commands;
using Revolt.Commands.Attributes;
using Revolt.Commands.Info;
using Revolt.Commands.Results;

namespace Taco.Attributes;

public class RequireServerModeratorAttribute : PreconditionAttribute
{
    public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
        IServiceProvider services)
    {
        var tContext = (TacoCommandContext)context;
        if(context.User._id == context.Server.OwnerId)
            return Task.FromResult(PreconditionResult.FromSuccess());
        foreach (var userRole in context.Server.GetMember(context.User._id).Roles ?? Array.Empty<string>())
        {
            if(tContext.ServerData.ModRoles.Contains(userRole))
                return Task.FromResult(PreconditionResult.FromSuccess());
        }
        return Task.FromResult(PreconditionResult.FromError("Not a server moderator."));
    }
}