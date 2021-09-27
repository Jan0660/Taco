using System;
using System.Threading.Tasks;
using Anargy.Attributes;
using Anargy.Info;
using Anargy.Results;
using Revolt;

namespace Anargy.Revolt.Preconditions
{
    public class RequireServerPermissionsAttribute : PreconditionAttribute
    {
        public ServerPermission Permissions { get; }
        public RequireServerPermissionsAttribute(ServerPermission permissions) => Permissions = permissions;

        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            var revContext = (RevoltCommandContext)context;
            foreach (var enumVal in Enum.GetValues<ServerPermission>())
            {
                var perms = revContext.Server.GetPermissionsFor(revContext.User._id);
                if (Permissions.HasFlag(enumVal))
                    if (!perms.Server.HasFlag(enumVal))
                    {
                        return PreconditionResult.FromError(
                            $"You need the {enumVal} server permission to execute this command.");
                    }
            }

            return PreconditionResult.FromSuccess();
        }
    }
}