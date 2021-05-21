using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;

namespace Taco.Util
{
    public static class IpApi
    {
        private static Regex _matchRegex = new("http(s?)://(.+)(/?.?)", RegexOptions.Compiled);

        public static async Task<IpApiResponse> GetAsync(string url)
        {
            if (_matchRegex.IsMatch(url))
            {
                url = url.Replace("http://", "").Replace("https://", "");
                if (url.Contains('/'))
                    url = url[..url.IndexOf('/')];
            }

            return JsonConvert.DeserializeObject<IpApiResponse>(
                (await new RestClient().ExecuteGetAsync(new RestRequest("http://ip-api.com/json/" + url))).Content,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
        }
    }

    public class IpApiResponse
    {
        public string Query;
        public string Status;
        public string Country;
        public string CountryCode;
        public string Region;
        public string RegionName;
        public string City;
        public string Zip;
        public double Lat;
        public double Lon;
        public string Timezone;
        public string Isp;
        public string Org;
        public string As;
    }
}