using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
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
using Revolt.Commands.Attributes;
using Revolt.Commands.Attributes.Preconditions;
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
        public async Task NuGetPackage(string name)
        {
            #region bloat

            CancellationToken cancellationToken = CancellationToken.None;
            SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            PackageMetadataResource resource = await repository.GetResourceAsync<PackageMetadataResource>();

            IEnumerable<IPackageSearchMetadata> packages = await resource.GetMetadataAsync(
                name,
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
        public Task HttpStatusCode(string query)
        {
            var code = httpCodes.Value.FirstOrDefault(c => c.Code.ToLower() == query.ToLower())
                       ?? httpCodes.Value.FirstOrDefault(c => c.Phrase.ToLower() == query.ToLower());
            if (code == null)
                return ReplyAsync(":x: Sorry, but I don't have information about this HTTP status code.");
            return ReplyAsync($@"> ## HTTP {code.Code} - {code.Phrase}
> {code.Description.Remove(0, 1).Remove(code.Description.Length - 2)}");
        }

        [Command("http-header")]
        public Task HttpHeaderInfo(string query)
        {
            var header = httpHeaders.Value.FirstOrDefault(h => h.Header.ToLower() == query.ToLower());
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
            var group = (GroupChannel)Message.Channel;
            return ReplyAsync($@"> ## {group.Name}
> {group.Description}
> **Owner:** [@{group.Owner.Username}](/@{group.OwnerId}) [`{group.OwnerId}`]
> **ID:** `{group._id}`
> {group.RecipientIds.Length} Recipients");
        }

        [Command("iplookup")]
        [Alias("hack", "ip")]
        [Summary("Retrieve information about an IP or domain.")]
        public async Task Hack(string query)
        {
            var res = await IpApi.GetAsync(query);
            // check for death response
            if (res.Query == query && res.Country == null)
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

        [Command("arch")]
        [Summary("Get package info from Arch Linux' official repositories.")]
        public async Task Arch(string name)
        {
            var search = await ArchReposApi.SearchPackage(name);
            if (!search.Results.Any())
            {
                await ReplyAsync("Sorry, no results.");
                return;
            }

            var pkg = search.Results.FirstOrDefault(p =>
                p.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
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

        public readonly static (string, string)[] CommonDistroNames = {
            ("pios", "Raspberry Pi OS"),
            ("raspios", "Raspberry Pi OS"),
            ("raspbian", "Raspberry Pi OS"),
            ("raspberry pi", "Raspberry Pi OS"),
            ("arch", "arch linux"),
            ("arhc", "arch linux"),
            ("intal", "Gentoo Linux"),
            ("intal gento", "Gentoo Linux"),
            ("based", "Gentoo Linux"),
            ("Independent", "Gentoo Linux"),
            ("gento", "Gentoo Linux"),
            ("openbased", "openbsd"),
            ("gentoo", "Gentoo Linux"),
        };

        public static LinuxDistro DistroSearch(string name)
            => linuxDistros.Value.FirstOrDefault(d => d.FullName.ToLower() == name.ToLower())
               ?? linuxDistros.Value.FirstOrDefault(d => d.DistributionFullName.ToLower() == name.ToLower())
               ?? linuxDistros.Value.FirstOrDefault(d => d.FullName.ToLowerTrimmed() == name.ToLower())
               ?? linuxDistros.Value.FirstOrDefault(d => d.FullName.ToLowerTrimmed() == name.ToLowerTrimmed());

        [Command("distro")]
        public Task LinuxDistro(string name)
        {
            var distro = DistroSearch(name);
            if (distro == null)
            {
                var endsWithLinux = !name.ToLowerTrimmed().EndsWith("linux")
                    ? linuxDistros.Value.FirstOrDefault(d =>
                    {
                        try
                        {
                            return d.FullName.ToLowerTrimmed().Remove(d.FullName.ToLowerTrimmed().Length - 5) ==
                                   name.ToLowerTrimmed();
                        }
                        catch
                        {
                            return false;
                        }
                    })
                    : null;
                if (endsWithLinux != null)
                    distro = endsWithLinux;

                var tryCommon = CommonDistroNames.FirstOrDefault(d => d.Item1.ToLower() == name.ToLower());
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
        public async Task SaveAttachment(string name)
        {
            if (Message.Attachments == null)
            {
                await ReplyAsync("You must provide an attachment!");
                return;
            }

            var attachment = Message.Attachments.First();
            Context.UserData.SavedAttachments.Add(name.ToLower(),
                $"https://autumn.revolt.chat/attachments/{attachment._id}/{HttpUtility.UrlEncode(attachment.Filename)}");
            await Context.UserData.UpdateAsync();
            await ReplyAsync("Attachment saved.");
        }

        [Command("unsave-attachment")]
        [Alias("unsaveAttachment", "us-a")]
        [Summary("Remove a saved attachment.")]
        public Task UnsaveAttachment(string name)
        {
            try
            {
                Context.UserData.SavedAttachments.Remove(name);
                return Context.UserData.UpdateAsync();
            }
            catch
            {
                return ReplyAsync("Attachment not found.");
            }
        }

        [Command("saved-attachment")]
        [Alias("savedAttachment", "s")]
        [Summary("Send a saved attachment.")]
        public Task SendSavedAttachment(string name)
        {
            if (Context.UserData.SavedAttachments.TryGetValue(name.ToLower(), out string url))
                return InlineReplyAsync(url);
            else
                return InlineReplyAsync("Saved attachment not found.");
        }
        [Command("savedAttachments")]
        [Alias("saved-attachments")]
        [Summary("List your saved attachments.")]
        public Task ListSavedAttachments()
        {
            var res = new StringBuilder();
            foreach (var att in Context.UserData.SavedAttachments)
                res.AppendLine($"- **[{att.Key}](<{att.Value}>)**");

            return InlineReplyAsync(res.ToString());
        }

        [Command("roles")]
        [Summary("Lists roles.")]
        [TextChannelOnly]
        public async Task RolesList()
        {
            await InlineReplyAsync("Commands with permissions temporarily disabled.");
            return;
//             var res = new StringBuilder();
//             res.AppendLine($@"**Default Channel Permissions:** {Context.Server.ChannelPermissions}");
//             res.AppendLine($@"**Default Server Permissions:** {Context.Server.ServerPermissions}");
//             res.AppendLine("\\");
//             foreach (var role in Context.Server.Roles)
//             {
//                 res.AppendLine(@$"$\color{{{role.Value.Color ?? "white"}}}\textsf{{{role.Value.Name}}}$
// > **Id:** `{role.Key}`");
//                 if (role.Value.Color != null)
//                     res.AppendLine($"> **Color:** `{role.Value.Color}`");
//                 var channelPerms = role.Value.ChannelPermissions ^
//                                    (Context.Server.ChannelPermissions & role.Value.ChannelPermissions);
//                 if (role.Value.ChannelPermissions != Context.Server.ChannelPermissions && channelPerms != 0)
//                     res.AppendLine(
//                         $"**Channel Permissions:** {channelPerms}");
//                 var serverPerms = role.Value.ServerPermissions ^
//                                   (Context.Server.ServerPermissions & role.Value.ServerPermissions);
//                 if (role.Value.ServerPermissions != Context.Server.ServerPermissions)
//                     res.AppendLine(
//                         $"**Server Permissions:** {serverPerms}");
//                 res.AppendLine("\n");
//             }
//
//             await ReplyAsync(res.ToString());
        }
    }
}