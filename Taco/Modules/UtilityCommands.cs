using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Anargy.Attributes;
using Anargy.Revolt.Preconditions;
using CryptoCurrencyApis;
using Jan0660.AzurAPINet.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using RestSharp;
using Revolt.Channels;
using Taco.Attributes;
using Taco.CommandHandling;
using Taco.Util;

namespace Taco.Modules
{
    [Name("Utility")]
    [Summary("Utilities")]
    public class UtilityCommands : TacoModuleBase
    {
        private static readonly SourceCacheContext Cache = new SourceCacheContext();
        private static readonly ILogger Logger = NullLogger.Instance;
        private static Lazy<HttpCode[]> httpCodes = new Lazy<HttpCode[]>(_getHttpCodes);
        private static Lazy<HttpHeader[]> httpHeaders = new Lazy<HttpHeader[]>(_getHttpHeaders);

        private static Lazy<LinuxDistro[]> linuxDistros = new(() =>
            JsonConvert.DeserializeObject<LinuxDistro[]>(File.ReadAllText("./Resources/LinuxDistros.json")));

        private static HttpHeader[] _getHttpHeaders()
        {
            var json = (new WebClient()).DownloadString(
                "https://raw.githubusercontent.com/for-GET/know-your-http-well/master/json/headers.json");
            return JsonConvert.DeserializeObject<HttpHeader[]>(json);
        }

        private static HttpCode[] _getHttpCodes()
        {
            var json = (new WebClient()).DownloadString(
                "https://raw.githubusercontent.com/for-GET/know-your-http-well/master/json/status-codes.json");
            return JsonConvert.DeserializeObject<HttpCode[]>(json);
        }

        // todo: if no exact match then display results
        [Command("nuget")]
        [Summary("Information about a package on NuGet.")]
        public async Task NuGetPackage()
        {
            #region bloat

            CancellationToken cancellationToken = CancellationToken.None;
            SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            PackageMetadataResource resource = await repository.GetResourceAsync<PackageMetadataResource>();

            IEnumerable<IPackageSearchMetadata> packages = await resource.GetMetadataAsync(
                Args,
                includePrerelease: false,
                includeUnlisted: false,
                Cache,
                Logger,
                cancellationToken);

            #endregion

            var packageSearchMetadatas = packages.ToList();
            if (!packageSearchMetadatas.Any())
            {
                await ReplyAsync(":x: No results.");
                return;
            }

            var pkg = packageSearchMetadatas.Last();
            // var versions = await pkg.GetVersionsAsync();
            await ReplyAsync($@"> # {pkg.Identity.Id} - v{pkg.Identity.Version.ToFullString()}
> {pkg.Description.Replace("\n", "\n> ")}
> **By:** {pkg.Authors}
> ## Dependencies:{new Func<string>(() => {
    var res = "";
    foreach (var target in pkg.DependencySets)
    {
        res += $@"
> ### {target.TargetFramework.GetDotNetFrameworkName(new DefaultFrameworkNameProvider())}";
        foreach (var dep in target.Packages) {
            res += $"\n> > {dep.Id} • {dep.VersionRange.PrettyPrint()}";
        }
    }

    return res;
}).Invoke()}" + $"\n> \n> [\\[.nupkg download\\]](https://api.nuget.org/v3-flatcontainer/{pkg.Identity.Id.ToLower()}/{pkg.Identity.Version.ToNormalizedString().ToLower()}/{pkg.Identity.Id.ToLower()}.{pkg.Identity.Version.ToNormalizedString().ToLower()}.nupkg)"
              + $" [\\[.nuspec download\\]](https://api.nuget.org/v3-flatcontainer/{pkg.Identity.Id.ToLower()}/{pkg.Identity.Version.ToNormalizedString().ToLower()}/{pkg.Identity.Id.ToLower()}.nuspec)"
            );
        }

        [Command("http")]
        [Alias("http-status-code", "status-code")]
        [Summary("Information about a HTTP status code.")]
        public Task HttpStatusCode()
        {
            var code = httpCodes.Value.FirstOrDefault(c => c.Code.ToLower() == Args.ToLower())
                       ?? httpCodes.Value.FirstOrDefault(c => c.Phrase.ToLower() == Args.ToLower());
            if (code == null)
                return ReplyAsync(":x: Sorry, but I don't have information about this HTTP status code.");
            return ReplyAsync($@"> ## HTTP {code.Code} - {code.Phrase}
> {code.Description.Remove(0, 1).Remove(code.Description.Length - 2)}");
        }

        [Command("http-header")]
        public Task HttpHeaderInfo()
        {
            var header = httpHeaders.Value.FirstOrDefault(h => h.Header.ToLower() == Args.ToLower());
            if (header == null)
                return ReplyAsync(":x: Sorry, but I don't have information about this HTTP header.");
            return ReplyAsync($@"> ## {header.Header}
> {(header.Description.StartsWith('\"') ? header.Description.Remove(0, 1).Remove(header.Description.Length - 2) : header.Description)}");
        }

        private class HttpCode
        {
            [JsonProperty("code")] public string Code;
            [JsonProperty("phrase")] public string Phrase;

            [JsonProperty("description")] public string Description;
            // spec_title
            // spec_href
        }

        private class HttpHeader
        {
            [JsonProperty("header")] public string Header;

            [JsonProperty("description")] public string Description;
            // spec_title
            // spec_href
        }

        [Command("group")]
        [Alias("groupInfo")]
        [Summary("Retrieves information about current group.")]
        [GroupOnly]
        public Task GroupInfo()
        {
            var group = (GroupChannel) Message.Channel;
            return ReplyAsync($@"> ## {group.Name}
> {group.Description}
> **Owner:** <@{group.OwnerId}> [`{group.OwnerId}`]
> **ID:** `{group._id}`
> {group.RecipientIds.Length} Recipients");
        }

        [Command("iplookup")]
        [Alias("hack", "ip")]
        [Summary("Retrieve information about an IP or domain.")]
        public async Task Hack()
        {
            var res = await IpApi.GetAsync(Args);
            // check for death response
            if (res.Query == Args && res.Country == null)
            {
                await ReplyAsync(":x: I can't find that IP or domain.");
                return;
            }

            await ReplyAsync(@$"> ## IP Lookup
> **IP:** {res.Query}
> **Country:** {res.Country} [{res.CountryCode}]
> **Region:** {res.RegionName}
> **City:** {res.City}
> **Zip:** {res.Zip}
> **Time zone:** {res.Timezone}
> **ISP:** {res.Isp}
> **Organization:** {res.Org}");
        }

        [Command("etc")]
        [Summary("Get ETC prices, wallet balance.")]
        public async Task Etc()
        {
            if (Args == "")
            {
                var rates = await EtherScanApi.GetRatesCached("ETC");
                await ReplyAsync(@$"> ## ETC Rates
> :us: USD: {rates.USD}
> :uk: GBP: {rates.GBP}
> :european_union: EUR: {rates.EUR}
> :czech_republic: CZK: {rates.CZK}");
            }
            else
            {
                var geth = new GethClient("https://blockscout.com/etc/mainnet/api/eth-rpc");
                var balance = await geth.GetBalance(Args);
                var rates = await EtherScanApi.GetRatesCached("ETC");
                await ReplyAsync($@"> ## ETC Wallet Balance
> ETC: {balance}
> :us: USD: {balance * rates.USD}
> :uk: GBP: {balance * rates.GBP}
> :european_union: EUR: {balance * rates.EUR}
> :czech_republic: CZK: {balance * rates.CZK}");
            }
        }

        [Command("arch")]
        [Summary("Get package info from Arch Linux' official repositories.")]
        public async Task Arch()
        {
            var search = await ArchReposApi.SearchPackage(Args);
            if (!search.Results.Any())
            {
                await ReplyAsync("Sorry, no results.");
                return;
            }

            var pkg = search.Results.FirstOrDefault(p =>
                p.Name.Equals(Args, StringComparison.InvariantCultureIgnoreCase));
            if (pkg == null)
            {
                // no exact match, show results
                await ReplyAsync($@"> # Arch Linux repos search
{new Func<string>(() => {
    var res = "";
    for (int i = 0; i < 5 && i < search.Results.Length; i++)
        res += @$"> #### {search.Results[i].Repository}/{search.Results[i].Name}
> > {search.Results[i].Description}
> 
";
    return res;
})()}> ##### {search.Results.Length} results");
            }
            else
                await ReplyAsync(@$"> # [{pkg.Repository}/{pkg.Name}]({pkg.PackageUrl})
> > {pkg.Description}
> 
> :woozy_face: **Dependencies:** {pkg.Depends.Length}
> :page_facing_up: **License(s):** {String.Join(", ", pkg.Licenses)}
> :floppy_disk: **Compressed Size:** {(pkg.CompressedSize >= 1000000
    ? $"{Math.Round(pkg.CompressedSize / 1000d / 1000, 2)}MB"
    : $"{pkg.CompressedSize / 1000}KB")}
> :floppy_disk: **Installed Size:** {(pkg.InstalledSize >= 1000000
    ? $"{Math.Round(pkg.InstalledSize / 1000d / 1000, 2)}MB"
    : $"{pkg.InstalledSize / 1000}KB")}");
        }

        public static (string, string)[] CommonDistroNames = new[]
        {
            ("pios", "Raspberry Pi OS"),
            ("raspios", "Raspberry Pi OS"),
            ("raspbian", "Raspberry Pi OS"),
            ("raspberry pi", "Raspberry Pi OS"),
            ("arch", "arch linux"),
            ("arhc", "arch linux"),
            ("the best distro there is", "Artix Linux"),
            ("the distro that best distro is based on", "Arch Linux"),
            ("the other best distro there is", "Gentoo Linux"),
            ("intal", "Gentoo Linux"),
            ("intal gento", "Gentoo Linux"),
            ("fuck systemd", "Artix Linux"),
            ("based", "Gentoo Linux"),
            ("Independent", "Gentoo Linux"),
            ("gento", "Gentoo Linux"),
            ("openbased", "openbsd"),
            ("systemd is bloat", "Artix Linux"),
            ("gentoo", "Gentoo Linux"),
            ("the fucking", "Gentoo Linux")
        };

        public static LinuxDistro DistroSearch(string name)
            => linuxDistros.Value.FirstOrDefault(d => d.FullName.ToLower() == name.ToLower())
               ?? linuxDistros.Value.FirstOrDefault(d => d.DistributionFullName.ToLower() == name.ToLower())
               ?? linuxDistros.Value.FirstOrDefault(d => d.FullName.ToLowerTrimmed() == name.ToLower())
               ?? linuxDistros.Value.FirstOrDefault(d => d.FullName.ToLowerTrimmed() == name.ToLowerTrimmed());

        [Command("distro")]
        public Task LinuxDistro()
        {
            var distro = DistroSearch(Args);
            if (distro == null)
            {
                var endsWithLinux = !Args.ToLowerTrimmed().EndsWith("linux")
                    ? linuxDistros.Value.FirstOrDefault(d =>
                    {
                        try
                        {
                            return d.FullName.ToLowerTrimmed().Remove(d.FullName.ToLowerTrimmed().Length - 5) ==
                                   Args.ToLowerTrimmed();
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    : null;
                if (endsWithLinux != null)
                    distro = endsWithLinux;

                var tryCommon = CommonDistroNames.FirstOrDefault(d => d.Item1.ToLower() == Args.ToLower());
                if (tryCommon.Item1 != null)
                    distro = DistroSearch(tryCommon.Item2.ToLower());

                if (distro == null)
                    return ReplyAsync("Couldn't find the distro.");
            }

            return ReplyAsync($@"> ## {distro.DistributionFullName}
> **Origin:** {distro.Origin}
> **Architectures:** {String.Join(", ", distro.Architectures)}
> **Status:** {distro.Status}
> **Desktops:** {String.Join(", ", distro.Desktops)}
> > {distro.Description}
> [\[Website\]]({distro.HomePage})");
        }

        [Command("save-attachment")]
        [Alias("saveAttachment", "s-a")]
        public async Task SaveAttachment()
        {
            if (Message.Attachments == null)
            {
                await ReplyAsync("You must provide an attachment!");
                return;
            }

            var attachment = Message.Attachments.First();
            Context.UserData.SavedAttachments.Add(Args.ToLower(),
                $"https://autumn.revolt.chat/attachments/{attachment._id}/{HttpUtility.UrlEncode(attachment.Filename)}");
            await Context.UserData.UpdateAsync();
            await ReplyAsync("Attachment saved.");
        }

        [Command("unsave-attachment")]
        [Alias("unsaveAttachment", "us-a")]
        public Task UnsaveAttachment()
        {
            try
            {
                Context.UserData.SavedAttachments.Remove(Args);
                return Context.UserData.UpdateAsync();
            }
            catch
            {
                return ReplyAsync("Attachment not found.");
            }
        }

        [Command("saved-attachment")]
        [Alias("savedAttachment", "s")]
        public Task SendSavedAttachment()
        {
            if (Context.UserData.SavedAttachments.TryGetValue(Args.ToLower(), out string url))
                return InlineReplyAsync(url);
            else
                return InlineReplyAsync("Saved attachment not found.");
        }
        // todo: commands to list saved attachments

        // get fucked lol
        [Command("invisible")]
        [GroupOnly]
        public async Task Invisible()
        {
            List<string> hiding = new();
            // todo: make thing only on group
            foreach (var user in await Message.Channel.GetMembersAsync())
            {
                if (user.Status.Presence == "Invisible")
                    hiding.Add(user.Username);
            }

            await ReplyAsync($@"> ## Invisible users:
> {String.Join("\n> @", hiding)}");
        }
    }
}