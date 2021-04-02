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
            sbyte level;
            if (!sbyte.TryParse(args.Last(), out level))
            {
                Enum.TryParse(args.Last(), ignoreCase: true, out PermissionLevel lvl);
                level = (sbyte)lvl;
            }

            var userData = Mongo.GetOrCreateUserData(userId);
            userData.PermissionLevel = (PermissionLevel)level;
            await userData.UpdateAsync();
            await ReplyAsync($"<@{userId}> [`{userId}`] permission level changed to `{level}`");
        }
    }
}