﻿using System.Threading.Tasks;
using Owoify;

namespace RevoltBot.Modules
{
    public class FunCommands : ModuleBase
    {
        [Command("owo")]
        public Task OwO()
            => ReplyAsync(Owoifier.Owoify(Args));
        
        
        [Command("uwu")]
        public Task UwU()
            => ReplyAsync(Owoifier.Owoify(Args, Owoifier.OwoifyLevel.Uwu));
        
        
        [Command("uvu")]
        public Task UvU()
            => ReplyAsync(Owoifier.Owoify(Args, Owoifier.OwoifyLevel.Uvu));
    }
}