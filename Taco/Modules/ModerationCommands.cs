using System.Threading.Tasks;
using Taco.Attributes;
using Taco.CommandHandling;

namespace Taco.Modules
{
    [Hidden]
    [RequireDeveloper]
    public class ModerationCommands : ModuleBase
    {
        [Command("logchannel")]
        [TextChannelOnly]
        public async Task SetLogChannel()
        {
            Context.ServerData.LogChannelId = Message.ChannelId;
            await Context.ServerData.UpdateAsync();
            await ReplyAsync("log channel set");
        }
    }
}