using System;
using System.Net.Mime;
using System.Threading.Tasks;
using Revolt.Commands.Info;
using Revolt.Commands.Results;

namespace Revolt.Commands.Attributes.Preconditions
{
    public class RequireBotServerPermissionAttribute : PreconditionAttribute
    {
        public ServerPermission Permissions { get; }
        public RequireBotServerPermissionAttribute(ServerPermission permissions) => Permissions = permissions;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            // foreach (var enumVal in Enum.GetValues<ServerPermission>())
            // {
            //     var perms = context.Server.GetPermissionsFor(context.Client.User._id);
            //     if (Permissions.HasFlag(enumVal))
            //         if (!perms.Server.HasFlag(enumVal))
            //         {
            //             return Task.FromResult(PreconditionResult.FromError(
            //                 $"I need the {enumVal} server permission to execute this command."));
            //         }
            // }
            
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}