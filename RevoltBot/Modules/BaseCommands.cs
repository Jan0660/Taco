using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RevoltApi.Channels;
using RevoltBot.Attributes;

namespace RevoltBot.Modules
{
    public class BaseCommands : ModuleBase
    {
        [Command("info")]
        public Task BotInformation()
            => ReplyAsync($@"> Taco
> **Developed by:** `owouwuvu` <@01EX40TVKYNV114H8Q8VWEGBWQ>
> **Latest update at:** {new FileInfo(Assembly.GetExecutingAssembly().Location).LastWriteTime.ToString("dd/mm/yyyy")}
> **Groups count:** {Message.Client.ChannelsCache.OfType<GroupChannel>().Count()}");

        [Command("help")]
        [Summary("HELP ME")]
        public async Task Help()
        {
            var description = @"Oh hey im too lazy to write a proper help command!!1!!!111!
Here's a table of shit you and I have no idea what
| Module | Description |
|:------- |:------:|
";
            foreach (var module in CommandHandler.ModuleInfos)
            {
                description += $"{module.Name} | {module.Summary ?? "no summary"}\n";
            }

            await ReplyAsync(description);
        }
    }
}