using System.Linq;
using System.Threading.Tasks;
using Jan0660.AzurAPINet;
using Jan0660.AzurAPINet.Enums;
using Taco.Attributes;
using Taco.CommandHandling;

namespace Taco.Modules
{
    [ModuleName("Azur Lane", "AzurLane", "az")]
    [Summary("ship, shipstats")]
    public class AzurLaneModule : ModuleBase
    {
        public static AzurAPIClient Azurlane = new();

        [Command("ship")]
        [Summary("Basic information about a ship.")]
        public Task Ship()
        {
            var ship = Azurlane.getShip(Args);
            if (ship == null)
                return ReplyAsync("Ship not found.");
            return Message.Channel.SendMessageAsync(
                @$"> # [{ship.Names.en ?? ship.Names.code} [{ship.Id}]]({ship.WikiUrl})
> **Nationality:** {ship.Nationality}
> **Class:** {ship.Class}
> **Build Time:** {(ship.Construction.Constructable ? ship.Construction.ConstructionTime.ToString("hh\\:mm\\:ss") : "Cannot be constructed")}
> **Rarity:** $\color{{{GetRarityColor(ship.GetRarityEnum().ToGeneralRarity())}}}\textsf{{{ship.Rarity}}}$ **|** $\color{{orange}}\textsf{{{ship.Stars.StarsString}}}$
> **Scrap Value:** {(ship.ScrapValue.CanScrap ? $@"{ship.ScrapValue.Coin} Coins; {ship.ScrapValue.Oil} Oil; {ship.ScrapValue.Medal} Medals;" : "Cannot be scraped")}
> ${{\footnotesize Footer {{\hspace{{1mm}}}} when?}}$");
        }

        [Command("shipImage", "shipImg")]
        [Summary("Get first skin for ship.")]
        public Task ShipFirstSkin()
        {
            var ship = Azurlane.getShip(Args);
            if (ship == null)
                return ReplyAsync("Ship not found.");
            return ReplyAsync(ship.Skins.First().Image);
        }

        [Command("shipstats")]
        [Summary("Level 0, 100, 120 (retrofit?) stats for a ship.")]
        public Task ShipStats()
        {
            var ship = Azurlane.getShip(Args);
            if (ship == null)
                return ReplyAsync("Ship not found.");
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

            return Message.Channel.SendMessageAsync(table); // , "bruh.png", data
        }

        /// <summary>
        /// Returns LaTeX color for the rarity.
        /// </summary>
        /// <returns></returns>
        public string GetRarityColor(Rarity rarity)
            => rarity switch
            {
                Rarity.Normal => "919191",
                Rarity.Rare => "51F3FF",
                Rarity.Elite => "DB13DB",
                Rarity.SuperRare => "OrangeRed",
                Rarity.UltraRare => "AAFFAA"
                //Rarity.UltraRare => "FFC0CB"
                //Rarity.UltraRare => "FF6D88"
            };
    }
}