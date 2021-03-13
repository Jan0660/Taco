using System.Threading.Tasks;
using Owoify;
using RevoltBot.Attributes;

namespace RevoltBot.Modules
{
    [ModuleName("Fun")]
    [Summary("r")]
    public class FunCommands : ModuleBase
    {
        [Command("owo")]
        [Summary("OwOify input.")]
        public Task OwO()
            => ReplyAsync(Owoifier.Owoify(Args));
        
        
        [Command("uwu")]
        [Summary("UwUify input.")]
        public Task UwU()
            => ReplyAsync(Owoifier.Owoify(Args, Owoifier.OwoifyLevel.Uwu));
        
        
        [Command("uvu")]
        [Summary("UvUify input.")]
        public Task UvU()
            => ReplyAsync(Owoifier.Owoify(Args, Owoifier.OwoifyLevel.Uvu));
    }
}