using System;
using System.Linq;
using System.Threading.Tasks;
using RevoltBot.Attributes;
using RevoltBot.CommandHandling;

namespace RevoltBot.Modules
{
    [ModuleName("Fuck you.")]
    [Summary("Fuck you.")]
    [Hidden]
    public class DevCommands : ModuleBase
    {
        [RequireDeveloper]
        [Command("dev perm")]
        public async Task SetPermissions()
        {
            var args = Args.Split(' ');
            var userId = args[0];
            if (userId == Program.BotOwnerId)
            {
                await ReplyAsync("Sussus amogus.");
                return;
            }
            sbyte level;
            if (!sbyte.TryParse(args.Last(), out level))
            {
                Enum.TryParse(args.Last(), ignoreCase: true, out PermissionLevel lvl);
                level = (sbyte)lvl;
            }

            if ((sbyte) Context.UserData.PermissionLevel <= level && Context.User._id != Program.BotOwnerId)
            {
                await ReplyAsync("Can't set higher or same permission level.");
                return;
            }

            var userData = Mongo.GetOrCreateUserData(userId);
            userData.PermissionLevel = (PermissionLevel)level;
            await userData.UpdateAsync();
            await ReplyAsync($"<@{userId}> [`{userId}`] permission level changed to `{level}`");
        }

        [Command("edittest")]
        [RequireDeveloper]
        public async Task EditTest()
        {
            var msg = await ReplyAsync("hell");
            for (int i = 0; i < 300; i++)
            {
                await msg.EditAsync(i.ToString());
                //await Task.Delay(100);
            }

            await msg.EditAsync("ok cool");
        }

        [Command("druh")]
        [Summary("rape webcocket")]
        [RequireBotOwner]
        public async Task Druh()
        {
            var delay = 30;
            while (true)
            {
                await Message.Channel.BeginTypingAsync();
                await Task.Delay(delay);
                await Message.Channel.EndTypingAsync();
                await Task.Delay(delay);
            }
        }

        [Command("dev annoy")]
        public async Task ToggleAnnoy()
        {
            Program.Config.AnnoyToggle = bool.Parse(Args.Split(' ').Last());
            await Program.SaveConfig();
            await ReplyAsync($"Toggled to `{Program.Config.AnnoyToggle}`.");
        }
    }
}