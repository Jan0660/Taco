using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Log73;
using Log73.ColorSchemes;
using Newtonsoft.Json;
using RestSharp;
using RevoltApi;
using RevoltApi.Channels;
using RevoltBot.Modules;
using Console = Log73.Console;

namespace RevoltBot
{
    public static class Program
    {
        public static string Prefix => Config.Prefix;
        public static Config Config;
        static async Task Main(string[] args)
        {
            Config = JsonConvert.DeserializeObject<Config>(await File.ReadAllTextAsync("./config.json"));
            #region no
            Console.Options.UseAnsi = false;
            Console.Options.ColorScheme = new RiderDarkMelonColorScheme();
            Console.Options.LogLevel = LogLevel.Debug;
            Console.Options.ObjectSerialization = ConsoleOptions.ObjectSerializationMethod.Json;
            var client = new RevoltClient(JsonConvert.DeserializeObject<Session>(await File.ReadAllTextAsync("./session.json")));
            var info = client.ApiInfo;
            Console.Info($"API Version: {info.Version}");
            client.OnReady += () =>
            {
                Console.Info($"Ready! Users: {client.UsersCache.Count}; Channels: {client.ChannelsCache.Count};");
                return Task.CompletedTask;
            };
            client.MessageReceived += message =>
            {
                if (message.Channel is DirectMessageChannel dm)
                    return dm.SendMessageAsync("fuck off");
                return Task.CompletedTask;
            };
            await client.ConnectWebSocketAsync();
            #endregion
            SnipeModule.Init(client);
            client.MessageReceived += ClientOnMessageReceived;
            CommandHandler.LoadCommands();
            var c = CommandHandler.Commands;
            await Task.Delay(-1);
        }

        private static Task ClientOnMessageReceived(Message message)
        {
            Console.Debug($"{message.Author.Username}[{message.AuthorId}] at [{message.ChannelId}] => {message.Content}");
            if (message.Content.StartsWith(Prefix))
            {
                try
                {
                    CommandHandler.ExecuteCommand(message, Prefix.Length);
                }
                catch(Exception exception)
                {
                    if (exception.Message == "COMMAND_NOT_FOUND")
                        return Task.CompletedTask;
                    message.Channel.SendMessageAsync($@"> ## Death occurred
> 
> ```csharp
> {exception.Message.Replace("\n", "\n> ")}
> ```");
                }
            }

            return Task.CompletedTask;
        }
    }
}