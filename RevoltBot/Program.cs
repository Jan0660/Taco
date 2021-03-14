using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Log73;
using Log73.ColorSchemes;
using Newtonsoft.Json;
using RevoltApi;
using RevoltApi.Channels;
using RevoltBot.Modules;
using Console = Log73.Console;

namespace RevoltBot
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

        static async Task Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            Config = JsonConvert.DeserializeObject<Config>(await File.ReadAllTextAsync("./config.json"));

            #region Logging configuration

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
            Console.Options.ObjectSerialization = ConsoleOptions.ObjectSerializationMethod.Json;

            #endregion

            var client =
                new RevoltClient(JsonConvert.DeserializeObject<Session>(await File.ReadAllTextAsync("./session.json")));
            var info = client.ApiInfo;
            Console.Info($"API Version: {info.Version}");

            #region Event handlers

            client.PacketReceived += (packetType, packet, message) =>
            {
                Console.Debug($"Message receive: Length: {message.Text.Length}; Type: {packetType};");
                return Task.CompletedTask;
            };
            client.PacketError += (packetType, packet, message, exception) =>
            {
                Console.Error(
                    @$"Packet error: message.Length: {message.Text.Length}; packetType: {packetType ?? "null"}; JObject parsed?: {packet != null};
exception.Message: {exception.Message}; exception.Source: {exception.Source};");
                return Task.CompletedTask;
            };
            client.OnReady += () =>
            {
                Console.Info($"Ready! Users: {client.UsersCache.Count}; Channels: {client.ChannelsCache.Count};");
                return Task.CompletedTask;
            };

            client.UserRelationshipUpdated += (userId, status) =>
            {
                if (status == RelationshipStatus.Incoming)
                {
                    return client.Users.AddFriendAsync(client.UsersCache.First(u => u._id == userId).Username);
                }

                return Task.CompletedTask;
            };
            client.MessageUpdated += (messageId, data) =>
            {
                Console.Info(
                    $"Message Updated: Id: {messageId}; NewContent: {data.Content}; Date: {data.Edited.Date};");
                return Task.CompletedTask;
            };

            #endregion

            await client.ConnectWebSocketAsync();

            SnipeModule.Init(client);
            client.MessageReceived += ClientOnMessageReceived;
            CommandHandler.LoadCommands();
            var r = await client.Users.GetRelationships();
            foreach (var h in r)
            {
                if (h.Status == RelationshipStatus.Incoming)
                    await client.Users.AddFriendAsync(client.UsersCache.First(u => u._id == h.UserId).Username);
            }

            StartTime = DateTime.Now;
            Console.Info($"Finished loading and connected in {stopwatch.ElapsedMilliseconds}ms.");
            await Task.Delay(-1);
        }

        private static Task ClientOnMessageReceived(Message message)
        {
            Console.Debug(
                $"{message.Author.Username}[{message.AuthorId}] at [{message.ChannelId}] => {message.Content}");
            if (message.Content.StartsWith(Prefix))
            {
                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        await CommandHandler.ExecuteCommandAsync(message, Prefix.Length);
                    }
                    catch (Exception exception)
                    {
                        Console.Exception(exception);
                        if (exception.Message == "One or more errors occurred. (COMMAND_NOT_FOUND)" |
                            exception.Message == "COMMAND_NOT_FOUND")
                            return Task.CompletedTask;
                        await message.Channel.SendMessageAsync($@"> ## Death occurred
> 
> ```csharp
> {exception.Message.Replace("\n", "\n> ")}
> ```");
                    }

                    return Task.CompletedTask;
                });
            }

            return Task.CompletedTask;
        }
    }
}