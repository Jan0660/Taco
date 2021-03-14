using System.Threading.Tasks;
using RevoltApi;
using RevoltApi.Channels;

namespace RevoltBot.Attributes
{
    public class RequireGroupOwnerAttribute : PreconditionAttribute
    {
        public override async Task<PreconditionResult> Evaluate(Message message)
        {
            if (message.Channel is GroupChannel group && group.OwnerId == message.AuthorId)
                return PreconditionResult.FromSuccess();
            return PreconditionResult.FromError("This command can only be ran by the owner of this group.");
        }
    }
}