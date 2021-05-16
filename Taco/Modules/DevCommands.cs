using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Taco.Attributes;
using Taco.CommandHandling;

namespace Taco.Modules
{
    [ModuleName("Fuck you.")]
    [Summary("Fuck you.")]
    [RequireDeveloper]
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

        [Command("dev updatestatus")]
        [Summary("Force update status.")]
        public async Task UpdateStatus()
        {
            await Annoy.Update();
            await ReplyAsync("Updated status and soige,o.");
        }

        [Command("dev setstatus")]
        public async Task SetStatus()
        {
            Program.Config.Status = _nullableArgs(Args);
            await Annoy.Update();
            await Program.SaveConfig();
            await ReplyAsync("UPdated le staustm,.");
        }

        [Command("dev setpresence")]
        public async Task SetPresence()
        {
            Program.Config.Presence = Args;
            await Annoy.Update();
            await Program.SaveConfig();
            await ReplyAsync("updated le presenc");
        }

        [Command("dev setprofile")]
        public async Task SetProfile()
        {
            Program.Config.Profile = _nullableArgs(Args);
            await Annoy.Update();
            await Program.SaveConfig();
            await ReplyAsync("profiel set!!!");
        }

        [Command("dev settimer")]
        public async Task SetTimer()
        {
            Program.Config.UpdateTime = int.Parse(Args);
            await Program.SaveConfig();
            await ReplyAsync($"Set le tiemr to `{Program.Config.UpdateTime}`,");
        }

        [Command("dev rateLimited")]
        [Summary("List people that got rate limited.")]
        public Task ListRateLimited()
        {
            var str = new StringBuilder();
            foreach (var retard in Program.RateLimited)
            {
                str.AppendLine($"> <@{retard.Key}>");
            }

            return ReplyAsync(str.ToString());
        }

        [Command("dev unRateLimit")]
        [Summary("Remove someone from the rate limit list.")]
        public Task RemoveRateLimited()
        {
            Program.RateLimited.Remove(Args);
            return ReplyAsync($"<@{Args}> has been removed from the rate limit list.");
        }

        private string _nullableArgs(string str) => str == "null" ? null : str;
    }
}