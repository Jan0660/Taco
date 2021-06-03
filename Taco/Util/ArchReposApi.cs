using System;
using System.Globalization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace Taco.Util
{
    public static class ArchReposApi
    {
        private static RestClient _restClient = new("https://archlinux.org/");

        public static async Task<ArchPackageFiles> GetPackageFiles(string repo, string arch, string name)
        {
            // example url: https://archlinux.org/packages/core/x86_64/coreutils/files/json/
            var req = new RestRequest($"/packages/{repo}/{arch}/{name}/files/json/");
            var res = await _restClient.ExecuteGetAsync(req);
            if (!res.IsSuccessful)
                throw new Exception("Request unsuccessful");
            return JsonConvert.DeserializeObject<ArchPackageFiles>(res.Content);
        }

        public static async Task<ArchInfoSearchResult> SearchPackage(string name)
        {
            // example url: https://archlinux.org/packages/search/json/?q=pacman
            var req = new RestRequest($"/packages/search/json/");
            req.AddQueryParameter("q", name);
            var res = await _restClient.ExecuteGetAsync(req);
            if (!res.IsSuccessful)
                throw new Exception("Request unsuccessful");
            return JsonConvert.DeserializeObject<ArchInfoSearchResult>(res.Content);
        }

        public static async Task<AurSearchFullResult> AurSearch(string query)
        {
            // example url: https://aur.archlinux.org/rpc/?v=5&type=search&arg=foobar
            var client = new RestClient("https://aur.archlinux.org/");
            var req = new RestRequest("/rpc");
            req.AddQueryParameter("v", "5");
            req.AddQueryParameter("type", "search");
            req.AddQueryParameter("arg", query);
            var response = await client.ExecuteGetAsync(req);
            return JsonConvert.DeserializeObject<AurSearchFullResult>(response.Content);
        }
    }

    public class ArchInfoSearchResult
    {
        [JsonProperty("version")] public int Version;

        [JsonProperty("limit")] public int Limit;

        [JsonProperty("valid")] public bool Valid;
        [JsonProperty("results")] public ArchPackageInfo[] Results;
        [JsonProperty("num_pages")] public int NumPages;
        [JsonProperty("page")] public int Page;
    }

    public class ArchPackageFiles
    {
        [JsonProperty("pkgname")] public string Name;
        [JsonProperty("repo")] public string Repository;
        [JsonProperty("arch")] public string Architecture;

        // ReSharper disable once InconsistentNaming
        [JsonProperty("pkg_last_update")] private string _pkg_last_update;
        // todo: test

        [JsonIgnore]
        public DateTime LastUpdate =>
            DateTime.ParseExact(_pkg_last_update, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);

        // ReSharper disable once InconsistentNaming
        [JsonProperty("files_last_update")] private string _files_last_update;

        // todo: test
        [JsonIgnore]
        public DateTime FilesLastUpdate =>
            DateTime.ParseExact(_files_last_update, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);

        [JsonProperty("files_count")] public int FilesCount;
        [JsonProperty("dir_count")] public int DirectoryCount;
        [JsonProperty("files")] public string[] Files;
    }

    public class ArchPackageInfo
    {
        [JsonProperty("pkgname")] public string Name;
        [JsonProperty("pkgbase")] public string Base;
        [JsonProperty("repo")] public string Repository;
        [JsonProperty("arch")] public string Architecture;
        [JsonProperty("pkgver")] public string Version;
        [JsonProperty("pkgrel")] public string PackageRel;
        [JsonProperty("epoch")] public int Epoch;
        [JsonProperty("pkgdesc")] public string Description;
        [JsonProperty("url")] public string Url;
        [JsonProperty("filename")] public string Filename;
        [JsonProperty("compressed_size")] public int CompressedSize;

        [JsonProperty("installed_size")] public int InstalledSize;

        // build_date
        // last_update
        [JsonProperty("last_update")] private string _lastUpdate;

        [JsonIgnore]
        public DateTime LastUpdate =>
            DateTime.ParseExact(_lastUpdate, "yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);

        // flag_date
        [JsonProperty("maintainers")] public string[] Maintainers;
        [JsonProperty("packager")] public string Packager;
        [JsonProperty("groups")] public string[] Groups;
        [JsonProperty("licenses")] public string[] Licenses;
        [JsonProperty("conflicts")] public string[] Conflicts;

        [JsonProperty("provides")] public string[] Provides;

        [JsonProperty("replaces")] public string[] Replaces;
        [JsonProperty("depends")] public string[] Depends;
        [JsonProperty("optdepends")] public string[] OptionalDepends;
        [JsonProperty("makedepends")] public string[] MakeDepends;
        [JsonProperty("checkdepends")] public string[] CheckDepends;

        [JsonIgnore] public string PackageUrl => $"https://archlinux.org/packages/{Repository}/{Architecture}/{Name}/";
    }

    public class AurSearchFullResult
    {
        public int version;
        public string type;
        public int resultcount;
        public AurSearchResult[] results;
        public string? error;
    }

    public class AurSearchResult
    {
        public int ID;
        public string Name;
        public int PackageBaseID;
        public string PackageBase;
        public string Version;
        public string Description;
        public string URL;
        public int NumVotes;
        public double Popularity;
        [JsonIgnore] public DateTimeOffset OutOfDate => DateTimeOffset.FromUnixTimeSeconds(OutOfDateRaw.Value);
        [JsonProperty("OutOfDate")] public long? OutOfDateRaw;
        public string Maintainer;
        [JsonIgnore] public DateTimeOffset FirstSubmitted => DateTimeOffset.FromUnixTimeSeconds(FirstSubmittedRaw);
        [JsonIgnore] public DateTimeOffset LastSubmitted => DateTimeOffset.FromUnixTimeSeconds(LastSubmittedRaw);
        [JsonProperty("FirstSubmitted")] public long FirstSubmittedRaw;
        [JsonProperty("LastModified")] public long LastSubmittedRaw;
        public string URLPath;
    }
}