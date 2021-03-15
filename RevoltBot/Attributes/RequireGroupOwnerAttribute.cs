using System.Threading.Tasks;
using RevoltApi;
using RevoltApi.Channels;
using RevoltBot.CommandHandling;

namespace RevoltBot.Attributes
{
    public class RequireGroupOwnerAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> Evaluate(Message message)
        {
            if (message.Channel is GroupChannel group && group.OwnerId == message.AuthorId)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("This command can only be ran by the owner of this group."));
        }
    }
}