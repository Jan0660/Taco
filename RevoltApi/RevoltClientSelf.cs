using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace RevoltApi
{
    public class RevoltClientSelf
    {
        public RevoltClient Client { get; }

        public RevoltClientSelf(RevoltClient client)
        {
            this.Client = client;
        }

        public Task EditProfileAsync(UserInfo info)
        {
            var req = new RestRequest($"/users/01EX40TVKYNV114H8Q8VWEGBWQ/", Method.PATCH);
            req.AddJsonBody(JsonConvert.SerializeObject(info));
            var res = Client._restClient.ExecuteAsync(req).Result;
            return Task.CompletedTask;
        }

        // todo: edit username and passwd thing route
    }

    public class UserInfo
    {
        [JsonProperty("status")] public Status Status;
        [JsonProperty("profile")] public Profile Profile;
    }

    public class Status
    {
        [JsonProperty("text")] public string Text;
        // todo: presence enum
        [JsonProperty("presence")] public string Presence;
    }

    public class Profile
    {
        [JsonIgnore]
        public string Content
        {
            get => content;
            set
            {
                if (value.Length >= ContentMaxLength)
                    throw new InvalidOperationException("sussus");
                content = value;
            }
        }

        [JsonProperty] private string content;
        public const int ContentMaxLength = 2000;
    }
}