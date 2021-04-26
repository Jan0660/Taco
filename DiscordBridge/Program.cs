using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Log73;
using Newtonsoft.Json;
using RestSharp;
using RevoltApi;
using Attachment = RevoltApi.Attachment;
using Console = Log73.Console;
using Meth = System.Math;

namespace DiscordBridge
{
    class Program
    {
        private static RevoltClient _client;
        public static RevoltClient Client => _client;
        public static Config Config;
        public static Dictionary<string, ulong> RevoltDiscordMessages = new();
        public static Dictionary<ulong, string> DiscordRevoltMessages = new();

        static async Task Main(string[] args)
        {
            Console.Options.UseAnsi = false;
            foreach (var msgType in MessageTypes.AsArray())
            {
                msgType.LogInfos.Add(new TimeLogInfo()
                {
                    Style = new()
                    {
                        Color = System.Drawing.Color.Gold
                    }
                });
            }
            Config = JsonConvert.DeserializeObject<Config>(await File.ReadAllTextAsync("./config.json"));
            _client = new RevoltClient(Config.RevoltSession);
            await _client.ConnectWebSocketAsync();
            _client.OnReady += () =>
            {
                Console.Log("Revolt ready!");
                return Task.CompletedTask;
            };
            var info = _client.ApiInfo;
            Console.Info($"API Version: {info.Version}");
            var discord = new DiscordWebhookClient(Config.WebhookId,
                Config.WebhookToken);
            _client.MessageReceived += async message =>
            {
                Console.Log(message.Content);
                if (message.AuthorId == Config.RevoltSession.UserId)
                    return;
                try
                {
                    var embeds = message.Attachment == null
                        ? null
                        : new List<Embed>()
                        {
                            new EmbedBuilder()
                            {
                                Title = message.Attachment.Filename,
                                ImageUrl = $"https://autumn.revolt.chat/attachments/{message.Attachment._id}/{message.Attachment.Filename}",
                                Color = new Color(47, 49, 54),
                                Footer = new()
                                {
                                    Text = $"{message.Attachment.SizeToString()} • {message.Attachment.Metadata.Width}x{message.Attachment.Metadata.Height}"
                                },
                                Url = $"https://autumn.revolt.chat/attachments/{message.Attachment._id}/{message.Attachment.Filename}"
                            }.Build()
                        };
                    var msg = await discord.SendMessageAsync(message.Content, username: message.Author.Username,
                        avatarUrl: message.Author.AvatarUrl, allowedMentions: new(), embeds: embeds);
                    RevoltDiscordMessages.Add(message._id, msg);
                }
                catch (Exception exc)
                {
                    Console.Exception(exc);
                }
            };
            _client.MessageUpdated += async (id, data) =>
            {
                if (RevoltDiscordMessages.TryGetValue(id, out ulong discordMessageId))
                {
                    await ModifyMessageAsync(discordMessageId, data.Content);
                }
            };
            var discordClient = new DiscordSocketClient();
            await discordClient.LoginAsync(TokenType.Bot, Config.DiscordBotToken);
            await discordClient.StartAsync();
            discordClient.Ready += () =>
            {
                Console.Log("Discord ready!");
                return Task.CompletedTask;
            };
            discordClient.MessageReceived += async message =>
            {
                if (message.Author.Id == Config.WebhookId)
                    return;
                if (message.Channel.Id != Config.DiscordChannelId)
                    return;
                if (message.Content == "-uber" && message.Author.IsBot == false)
                {
                    await message.Channel.SendFileAsync("../Taco/Resources/UberFruit.png");
                }
                try
                {
                    var msg = await _client.Channels.SendMessageAsync(Config.RevoltChannelId,
                        message.ToGoodString());
                    DiscordRevoltMessages.Add(message.Id, msg._id);
                }
                catch (Exception exc)
                {
                    Console.Exception(exc);
                }
            };
            discordClient.MessageUpdated += async (cacheable, message, channel) =>
            {
                if (DiscordRevoltMessages.TryGetValue(cacheable.Id, out string revoltMessageId))
                {
                    await _client.Channels.EditMessageAsync(Config.RevoltChannelId, revoltMessageId, message.ToGoodString());
                }
            };
            await Task.Delay(-1);
        }

        public static Task ModifyMessageAsync(ulong id, string newContent)
        {
            var restClient =
                new RestClient($"https://discord.com/api/webhooks/{Config.WebhookId}/{Config.WebhookToken}/messages/{id}");
            var req = new RestRequest(Method.PATCH);
            req.AddJsonBody(JsonConvert.SerializeObject(new
            {
                content = newContent
            }));
            var h = restClient.ExecuteAsync(req).Result;
            return Task.CompletedTask;
        }
    }

    public class Config
    {
        public ulong WebhookId;
        public string WebhookToken;
        public string DiscordBotToken;
        public ulong DiscordChannelId;
        public string RevoltChannelId;
        public Session RevoltSession;
    }

    public static class ExtensionMethods
    {
        public static string ToBetterString(this SocketUser user)
        {
            var str = user.Discriminator == "0000" ? user.Username : user.ToString();
            if (str.StartsWith("# "))
                str = '\\' + str;
            str = str.Replace("$", "\\$");
            return str;
        }

        public static string ToGoodString(this SocketMessage message)
            => message.Author.ToBetterString() + "> " + message.Content;

        public static string SizeToString(this Attachment att)
        {
            var size = (decimal)att.Size;
            if (size > 1_000_000)
                return Meth.Round(size / 1000 / 1000, 2) + "MB";
            if (size > 1_000)
                return Meth.Round(size / 1000, 2) + "KB";
            return size + "B";
        }
    }
}