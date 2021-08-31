using System.Threading.Tasks;
using Revolt;
using Taco.CommandHandling;

namespace Taco.Attributes
{
    public class RequireDeveloperAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> Evaluate(CommandContext context)
        {
            var data = Mongo.GetUserData(context.Message.AuthorId);
            if (data is {PermissionLevel: (PermissionLevel)100})
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("You suck."));
        }
    }
}