using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Jan0660.AzurAPINet;
using RevoltBot.Attributes;

namespace RevoltBot.Modules
{
    [ModuleName("Azur Lane")]
    [Summary("ship, shipstats")]
    public class AzurLaneModule : ModuleBase
    {
        public static AzurAPIClient Azurlane = new();

        [Command("ship")]
        public async Task Ship()
        {
            var ship = Azurlane.getShip(Args);
            // var web = new WebClient();
            // var data = await web.DownloadDataTaskAsync(ship.Skins.First().Image);
            await Message.Channel.SendMessageAsync(
                @$"> # [{ship.Names.en ?? ship.Names.code} [{ship.Id}]]({ship.WikiUrl})
> **Nationality:** {ship.Nationality}
> **Class:** {ship.Class}
> **Build Time:** {(ship.Construction.Constructable ? ship.Construction.ConstructionTime.ToString("hh\\:mm\\:ss") : "Cannot be constructed")}
> **Rarity:** {ship.Rarity} **|** $\color{{orange}}\text{{{ship.Stars.StarsString}}}$
> **Scrap Value:** {(ship.ScrapValue.CanScrap ? $@"{ship.ScrapValue.Coin} Coins; {ship.ScrapValue.Oil} Oil; {ship.ScrapValue.Medal} Medals;" : "Cannot be scraped")}
> ${{\footnotesize Footer {{\hspace{{1mm}}}} when?}}$"); // , "bruh.png", data
        }

        [Command("shipstats")]
        public async Task ShipStats()
        {
            var ship = Azurlane.getShip(Args);
            // var data = await web.DownloadDataTaskAsync(ship.Skins.First().Image);
            var table =
                @"| Thing | Base | L.100 | L.120 |
|:------- |:------:|:-------:|:-------:|
";
            if (ship.Retrofittable)
            {
                table =
                    @"| Thing | Base | L.100 | L.100R | L.120 | L.120R |
|:------- |:------:|:-------:|:-------:|:-------:|:-------:|
";
            }
            var baseStats = ship.Stats.BaseStats.ToDict();
            var lvl100Stats = ship.Stats.Level100.ToDict();
            var lvl120Stats = ship.Stats.Level120.ToDict();
            var lvl100RetrofitStats = ship.Stats.Level100Retrofit?.ToDict();
            var lvl120RetrofitStats = ship.Stats.Level120Retrofit?.ToDict();
            foreach (var baseStat in baseStats)
            {
                if (ship.Retrofittable)
                    table +=
                        $@"| {baseStat.Key} | {baseStat.Value} | {lvl100Stats[baseStat.Key]} | {lvl100RetrofitStats![baseStat.Key]} | {lvl120Stats[baseStat.Key]} | {lvl120RetrofitStats![baseStat.Key]} |" +
                        '\n';
                else
                    table +=
                        $@"| {baseStat.Key} | {baseStat.Value} | {lvl100Stats[baseStat.Key]} | {lvl120Stats[baseStat.Key]} |" +
                        '\n';
            }

            await Message.Channel.SendMessageAsync(table); // , "bruh.png", data
        }
    }
}