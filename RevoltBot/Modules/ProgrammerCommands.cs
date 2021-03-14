using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuGet.Common;
using NuGet.Frameworks;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using RevoltBot.Attributes;

namespace RevoltBot.Modules
{
    [ModuleName("Code", "Programmer", "Nerd")]
    [Summary("Utilities for programmers.")]
    public class ProgrammerCommands : ModuleBase
    {
        private static readonly SourceCacheContext Cache = new SourceCacheContext();
        private static readonly ILogger Logger = NullLogger.Instance;
        private static Lazy<HttpCode[]> httpCodes = new Lazy<HttpCode[]>(_getHttpCodes);
        private static Lazy<HttpHeader[]> httpHeaders = new Lazy<HttpHeader[]>(_getHttpHeaders);

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
                throw new Exception("no results");
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

        [Command("http", "http-status-code", "status-code")]
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
    }
}