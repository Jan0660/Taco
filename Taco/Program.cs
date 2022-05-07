using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Log73;
using Log73.ColorSchemes;
using Log73.Extensions;
using Newtonsoft.Json;
using NuGet.Packaging;
using Revolt;
using Revolt.Channels;
using Taco.CommandHandling;
using Taco.Modules;
using Console = Log73.Console;

namespace Taco
{
    public static class Program
    {
#if DEBUG
        public const string Prefix = "[";
#else
        public const string Prefix = "-";
#endif
        public static Config Config;
        public const string BotOwnerId = "01EX40TVKYNV114H8Q8VWEGBWQ";
        public static DateTime StartTime { get; private set; }
        private static RevoltClient _client;

        public static RevoltClient Client => _client;
        // public const int RateLimitTriggerDuration = 3000;
        //
        // /// <summary>
        // /// Number of messages to trigger rate limit in <see cref="RateLimitTriggerDuration"/> milliseconds.
        // /// </summary>
        // public const int ToTriggerRateLimit = 3;
        //
        // /// <summary>
        // /// Rate limit duration in milliseconds.
        // /// </summary>
        // public const int RateLimitDuration = 30000;
        //
        // public static Dictionary<string, DateTime> RateLimited = new();
        // public static Dictionary<string, (DateTime Start, int Times)> RateLimit = new();

        public const string CocMatchPrefix =
#if !DEBUG
                "coc#"
#else
                "cock"
#endif
            ;

        public static readonly Regex CocMatchRegex = new(CocMatchPrefix + "[0-9]{1,2}", RegexOptions.Compiled);

        static async Task Main()
        {
            TaskScheduler.UnobservedTaskException += (_, eventArgs) => { Console.Exception(eventArgs.Exception); };
            var stopwatch = Stopwatch.StartNew();
            Config = JsonConvert.DeserializeObject<Config>(await File.ReadAllTextAsync("./config.json"))!;

            _configureLogging();

            _client = new RevoltClient();
            await _client.LoginAsync(TokenType.Bot, Config!.BotToken);

            // var ch = _client.Channels.Get("01F7ZSBSFHCAAJQ92ZGTY67HMN");
            //
            // return;

            #region Event handlers

            // _client.PacketReceived += (packetType, packet, message) =>
            // {
            //     Console.Debug($"Packet receive: Length: {message.Text.Length}; Type: {packetType};");
            //     return Task.CompletedTask;
            // };
            _client.PacketError += (packetType, packet, message, exception) =>
            {
                Console.Error(
                    @$"Packet error: message.Length: {message.Text.Length}; packetType: {packetType ?? "null"}; JObject parsed?: {packet != null};
exception.Message: {exception.Message}; exception.Source: {exception.Source};");
                return Task.CompletedTask;
            };
            _client.OnReady += () =>
            {
                Console.Info(
                    $"Ready! Users: {_client.UsersCache.Count}; Channels: {_client.ChannelsCache.Count}; Servers: {_client.ServersCache.Count};");
                var stopwatch = Stopwatch.StartNew();
                _client.CacheAll().ContinueWith(_ =>
                {
                    var membersCacheCount = 0;
                    foreach (var server in _client.ServersCache)
                        membersCacheCount += server.MemberCache.Count;
                    Console.Info($"Members and users cached in {stopwatch.ElapsedMilliseconds}ms! Count: {membersCacheCount}");
                });
                return Task.CompletedTask;
            };
            ServerLogging.RegisterEvents();

            #endregion

            await _client.ConnectWebSocketAsync();
            var info = _client.ApiInfo;
            Console.Info($"API Version: {info.Version}");

            SnipeModule.Init(_client);
            _client.MessageReceived += ClientOnMessageReceived;
            await CommandHandler.InitializeAsync();

            StartTime = DateTime.Now;
            Console.Info($"Finished loading and connected in {stopwatch.ElapsedMilliseconds}ms.");
            Console.Info("Connecting to MongoDB...");
            await Mongo.Connect();
            Console.Info("Connected to MongoDB.");
            await Client.Self.EditProfileAsync(new UserInfo()
            {
                Status = new()
                {
                    Text = Config!.Status,
                    Presence = Config.Presence
                },
                Profile = new()
                {
                    Content = Config.Profile
                }
            });
            await Task.Delay(-1);
        }

        private static async Task ClientOnMessageReceived(Message message)
        {
            // coc
            var cocMatch = CocMatchRegex.Match(message.Content);
            if (cocMatch.Success && message.Channel is TextChannel { ServerId: "01F7ZSBSFHQ8TA81725KQCSDDP" })
                await message.Channel.SendMessageAsync(
                    Config.CodeOfConduct[int.Parse(cocMatch.Value[CocMatchPrefix.Length..])]);
        }

        public static Task SaveConfig()
            => File.WriteAllTextAsync("./config.json", JsonConvert.SerializeObject(Config));

        private static void _configureLogging()
        {
#if DEBUG
            Console.Options.UseAnsi = false;
            Console.Options.ColorScheme = new RiderDarkMelonColorScheme();
#endif
#if DEBUG
            MessageTypes.Debug.Style.Color = Color.Pink;
#else
            MessageTypes.Debug.Style.Color = Color.FromArgb(255, 191, 254);
#endif
            Console.Options.LogLevel = LogLevel.Debug;
            Console.Configure.UseNewtonsoftJson();
            var msgTypes = MessageTypes.AsArray();
            var timeLogInfo = new TimeLogInfo()
            {
                Style = new() { Color = Color.Gold }
            };
            foreach (var msgType in msgTypes)
            {
                msgType.LogInfos.Add(timeLogInfo);
            }
        }
    }
}