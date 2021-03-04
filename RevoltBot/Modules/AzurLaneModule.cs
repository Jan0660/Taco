using System.Net;
using System.Threading.Tasks;
using Jan0660.AzurAPINet;

namespace RevoltBot.Modules
{
    public class AzurLaneModule : ModuleBase
    {
        public static AzurAPIClient Azurlane = new ();
        [Command("ship")]
        public async Task Ship()
        {
            var ship = Azurlane.getShip(Args);
            var web = new WebClient();
            var data = await web.DownloadDataTaskAsync(ship.Thumbnail);
            await Message.Channel.SendFileAsync("h", "bruh.png", data);
        }
    }
}