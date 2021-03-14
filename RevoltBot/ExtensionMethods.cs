using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Jan0660.AzurAPINet.Ships;
using RevoltApi;
using RevoltApi.Channels;
using RevoltBot.Attributes;
using SixLabors.ImageSharp;

namespace RevoltBot
{
    public static class ExtensionMethods
    {
        public static async Task<Message> SendPngAsync(this Channel channel, Image image, string content, string filename = "h.png")
        {
            var memory = new MemoryStream();
            await image.SaveAsPngAsync(memory);
            return await channel.SendFileAsync(content, filename, memory.GetBuffer());
        }
        
        /*
 @$"**Health:** {stats.Health}
**Armor:** {stats.Armor}
**Reload:** {stats.Reload}
**Luck:** {stats.Luck}
**Firepower:** {stats.Firepower}
**Torpedo:** {stats.Torpedo}
**Evasion:** {stats.Evasion}
**Speed:** {stats.Speed}
**AntiAir:** {stats.AntiAir}
**Aviation:** {stats.Aviation}
**Oil consumption:** {stats.OilConsumption}
**Accuracy:** {stats.Accuracy}
**AntiSubmarineWarfare:** {stats.AntiSubmarineWarfare}",
 */
        public static Dictionary<string, string> ToDict(this ShipStats stats)
        {
            var dict = new Dictionary<string, string>();
            dict.Add("Oil consumption", stats.OilConsumption.ToString());
            dict.Add("Health", stats.Health.ToString());
            dict.Add("Armor", stats.Armor);
            dict.Add("Reload", stats.Reload.ToString());
            dict.Add("Luck", stats.Luck.ToString());
            dict.Add("Firepower", stats.Firepower.ToString());
            dict.Add("Torpedo", stats.Torpedo.ToString());
            dict.Add("Evasion", stats.Evasion.ToString());
            dict.Add("Speed", stats.Speed.ToString());
            // AA
            dict.Add("AntiAir", stats.AntiAir.ToString());
            dict.Add("Aviation", stats.Aviation.ToString());
            dict.Add("Accuracy", stats.Accuracy.ToString());
            // ASW
            dict.Add("AntiSub", stats.AntiSubmarineWarfare.ToString());
            return dict;
        }
    }
}