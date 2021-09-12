using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;
using Revolt.Channels;
using Websocket.Client;

#pragma warning disable 4014

namespace Revolt
{
    public enum RevoltClientState
    {
        NotLoggedIn,
        LoggedIn,
        WebSocketConnected,
        WebSocketReady,
    }

    public partial class RevoltClient
    {
        // todo: temporary
        private const string _defaultApiUrl = "https://api.revolt.chat/";
        internal RestClient _restClient;
        public RevoltApiInfo ApiInfo { get; private set; }
        public AutumnInformation AutumnInfo { get; private set; }
        internal WebsocketClient? _webSocket { get; private set; }
        public RevoltClientState State { get; private set; } = RevoltClientState.NotLoggedIn;
        internal List<User> _users = new();
        public IReadOnlyList<User> UsersCache => _users.AsReadOnly();
        private List<Channel> _channels = new();
        public IReadOnlyList<Channel> ChannelsCache => _channels.AsReadOnly();

        public List<Server> ServersCache { get; internal set; }
        private Timer _pingTimer;
        public string ApiUrl { get; private set; } = _defaultApiUrl;
        public string AutumnUrl => ApiInfo!.Features.Autumn.Url;
        public string VortexUrl => ApiInfo!.Features.Vortex.Url;

        public RevoltClientChannels Channels { get; private set; }
        public RevoltClientUsers Users { get; private set; }
        public RevoltClientSelf Self { get; private set; }
        public RevoltClientServers Servers { get; private set; }
        private static Random rng = new();
        public TokenType TokenType { get; private set; }
        private string token;

        /// <summary>
        /// Create an unauthenticated client, use <see cref="LoginAsync"/> for authenticating.
        /// </summary>
        public RevoltClient(string apiUrl = _defaultApiUrl)
        {
            ApiUrl = apiUrl;
            _restClient = new(_defaultApiUrl);
            this.Channels = new RevoltClientChannels(this);
            this.Users = new RevoltClientUsers(this);
            this.Self = new RevoltClientSelf(this);
            this.Servers = new RevoltClientServers(this);
        }

        /// <summary>
        /// Log in with an existing token and finish initialization of the client.
        /// </summary>
        /// <param name="tokenType"></param>
        /// <param name="token">User or bot token.</param>
        /// <param name="userId">(will not be needed in the future)</param>
        public async Task LoginAsync(TokenType tokenType, string token, string userId)
        {
            ApiInfo = await GetApiInfoAsync();
            AutumnInfo = await GetAutumnInfoAsync();
            _useToken(tokenType, token, userId);
        }

        private void _useToken(TokenType tokenType, string token, string userId)
        {
            Self.UserId = userId;
            this.token = token;
            TokenType = tokenType;
            if (tokenType == TokenType.User)
                _restClient.AddDefaultHeader("x-session-token", token);
            else if (tokenType == TokenType.Bot)
                _restClient.AddDefaultHeader("x-bot-token", token);
            State = RevoltClientState.LoggedIn;
        }

        /// <summary>
        /// Creates a session and automatically uses it.
        /// </summary>
        /// <returns>The session that was created.</returns>
        public async Task<Session> LoginAsync(string email, string password, string? deviceName = null,
            string? captcha = null)
        {
            var session = await _requestAsync<Session>($"{ApiUrl}/auth/login", body: JsonConvert.SerializeObject(new
            {
                email, password, device_name = deviceName, captcha
            }));
            _useToken(TokenType.User, session.SessionToken, session.UserId);
            return session;
        }

        private void ThrowIfNotLoggedIn(string msg = "Revolt client is not logged in!")
        {
            if (State == RevoltClientState.NotLoggedIn)
                throw new Exception(msg);
        }

        public async Task<string> UploadFile(string name, string path, string tag = "attachments")
        {
            var aut = new RestClient(AutumnUrl);
            var req = new RestRequest($"/{tag}");
            req.AddFile(name, path);
            var res = await aut.ExecutePostAsync(req);
            var obj = JObject.Parse(res.Content);
            return obj.Value<string>("id");
        }

        public async Task<string> UploadFile(string name, byte[] data, string tag = "attachments")
        {
            var aut = new RestClient(AutumnUrl);
            var req = new RestRequest($"/{tag}");
            req.AddFile(name, data, name);
            var res = await aut.ExecutePostAsync(req);
            var obj = JObject.Parse(res.Content);
            return obj.Value<string>("id");
        }

        public Task<IRestResponse> UpdateAvatarId(string id)
        {
            var req = new RestRequest("/users/@me", Method.PATCH);
            req.AddJsonBody(JsonConvert.SerializeObject(new
            {
                avatar = id
            }));
            return _restClient.ExecuteAsync(req);
        }

        public async Task<VortexInformation> GetVortexInfoAsync()
            => JsonConvert.DeserializeObject<VortexInformation>(
                await new HttpClient().GetStringAsync(VortexUrl))!;

        public async Task<AutumnInformation> GetAutumnInfoAsync()
            => JsonConvert.DeserializeObject<AutumnInformation>(
                await new HttpClient().GetStringAsync(AutumnUrl))!;

        public async Task<RevoltApiInfo> GetApiInfoAsync()
            => JsonConvert.DeserializeObject<RevoltApiInfo>(await new HttpClient().GetStringAsync(ApiUrl))!;

        public static string GenerateNonce()
            => DateTimeOffset.Now.ToUnixTimeMilliseconds() + rng.Next().ToString();

        internal void CacheChannel(Channel channel)
        {
            var cached = _channels.FirstOrDefault(c => c._id == channel._id);
            if (cached != null)
                _channels.Remove(cached);
            _channels.Add(channel);
        }
    }

    public class SendMessageRequest
    {
        [JsonProperty("content")] public string Content;
        [JsonProperty("nonce")] public string Nonce = RevoltClient.GenerateNonce();
        [JsonProperty("attachments")] public List<string>? Attachments;
        [JsonProperty("replies")] public MessageReply[] Replies;
    }

    public struct MessageReply
    {
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("mention")] public bool Mention { get; set; }

        public MessageReply(string id, bool mention = false) => (Id, Mention) = (id, mention);
    }

    public class RevoltException : Exception
    {
        public RevoltError Error { get; }

        public RevoltException(RevoltError error, IRestResponse response) : base(
            $"Revolt responded with {(int)response.StatusCode}: {error.Type}.") => (Error) = (error);
    }

    public class RevoltError
    {
        [JsonProperty("type")] public string Type { get; set; }
    }

    internal static class RevoltInternalExtensionMethods
    {
        public static User AttachClient(this User user, RevoltClient client)
        {
            user.Client = client;
            if (user.Avatar != null)
                user.Avatar.Client = client;
            return user;
        }

        public static User[] AttachClient(this User[] users, RevoltClient client)
        {
            foreach (var user in users)
            {
                user.Client = client;
                if (user.Avatar != null)
                    user.Avatar.Client = client;
            }

            return users;
        }
    }

    public enum TokenType
    {
        Bot,
        User
    }
}