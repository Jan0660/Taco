using System;
using System.Threading.Tasks;
using Revolt;
using Taco.CommandHandling;

namespace Taco.Attributes
{
    public class RequireServerPermissionsAttribute : PreconditionAttribute
    {
        public ServerPermission Permissions { get; }
        public RequireServerPermissionsAttribute(ServerPermission permissions) => Permissions = permissions;

        public override async Task<PreconditionResult> Evaluate(CommandContext context)
        {
            foreach (var enumVal in Enum.GetValues<ServerPermission>())
            {
                if (Permissions.HasFlag(enumVal))
                    if (!(await context.GetServerPermissionsAsync()).HasFlag(enumVal))
                    {
                        return PreconditionResult.FromError(
                            $"You need the {enumVal} server permission to execute this command.");
                    }
            }

            return PreconditionResult.FromSuccess();
        }
    }
}