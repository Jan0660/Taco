using System;
using System.Threading.Tasks;
using Revolt.Commands.Info;
using Revolt.Commands.Results;

namespace Revolt.Commands.Attributes.Preconditions
{
    public class RequireServerPermissionsAttribute : PreconditionAttribute
    {
        public ServerPermission Permissions { get; }
        public RequireServerPermissionsAttribute(ServerPermission permissions) => Permissions = permissions;

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            return Task.FromResult(PreconditionResult.FromError("Temporarily disabled."));
            // foreach (var enumVal in Enum.GetValues<ServerPermission>())
            // {
            //     var perms = context.Server.GetPermissionsFor(context.User._id);
            //     if (Permissions.HasFlag(enumVal))
            //         if (!perms.Server.HasFlag(enumVal))
            //         {
            //             return Task.FromResult(PreconditionResult.FromError(
            //                 $"You need the {enumVal} server permission to execute this command."));
            //         }
            // }
            //
            // return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}