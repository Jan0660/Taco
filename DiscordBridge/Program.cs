using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Log73;
using Newtonsoft.Json;
using RestSharp;
using RevoltApi;
using RevoltApi.Channels;
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
        public static Regex ReplaceRevoltMentions = new("<@([0-9,A-Z]{26})+>", RegexOptions.Compiled);

        static async Task Main(string[] args)
        {
#if DEBUG
            Console.Options.UseAnsi = false;
#endif
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

            Console.Log("h");

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
                if (message.Channel is not GroupChannel)
                    return;
                if (message.ChannelId != Config.RevoltChannelId)
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
                                ImageUrl =
                                    $"https://autumn.revolt.chat/attachments/{message.Attachment._id}/{message.Attachment.Filename}",
                                Color = new Color(47, 49, 54),
                                Footer = new()
                                {
                                    Text =
                                        $"{message.Attachment.SizeToString()} • {message.Attachment.Metadata.Width}x{message.Attachment.Metadata.Height}"
                                },
                                Url =
                                    $"https://autumn.revolt.chat/attachments/{message.Attachment._id}/{message.Attachment.Filename}"
                            }.Build()
                        };
                    var msg = await discord.SendMessageAsync(message.Content.ReplaceRevoltMentions(),
                        username: message.Author.Username,
                        avatarUrl: message.Author.Avatar == null ? message.Author.DefaultAvatarUrl : message.Author.AvatarUrl + "?size=256", allowedMentions: new(), embeds: embeds);
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
                    await ModifyMessageAsync(discordMessageId, data.Content.ReplaceRevoltMentions());
                }
            };
            _client.MessageDeleted += (messageId) =>
            {
                if (RevoltDiscordMessages.TryGetValue(messageId, out ulong id))
                {
                    return DeleteMessageAsync(id);
                }

                return Task.CompletedTask;
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
                    Message msg;
                    if (message.Attachments.Any())
                    {
                        var http = new HttpClient();
                        var attachment = await http.GetByteArrayAsync(message.Attachments.First().ProxyUrl);
                        var attachmentId = await _client.UploadFile(message.Attachments.First().Filename, attachment);
                        msg = await _client.Channels.SendMessageAsync(Config.RevoltChannelId, message.ToGoodString(),
                            attachmentId);
                    }
                    else
                        msg = await _client.Channels.SendMessageAsync(Config.RevoltChannelId,
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
                if (message.Content == null)
                    return;
                if (DiscordRevoltMessages.TryGetValue(cacheable.Id, out string revoltMessageId))
                {
                    await _client.Channels.EditMessageAsync(Config.RevoltChannelId, revoltMessageId,
                        message.ToGoodString());
                }
            };
            discordClient.MessageDeleted += (cacheable, channel) =>
            {
                if (DiscordRevoltMessages.TryGetValue(cacheable.Id, out string id))
                {
                    return _client.Channels.DeleteMessageAsync(Config.RevoltChannelId, id);
                }

                return Task.CompletedTask;
            };
            await Task.Delay(-1);
        }

        public static Task ModifyMessageAsync(ulong id, string newContent)
        {
            var restClient =
                new RestClient(
                    $"https://discord.com/api/webhooks/{Config.WebhookId}/{Config.WebhookToken}/messages/{id}");
            var req = new RestRequest(Method.PATCH);
            req.AddJsonBody(JsonConvert.SerializeObject(new
            {
                content = newContent
            }));
            var h = restClient.ExecuteAsync(req).Result;
            return Task.CompletedTask;
        }

        public static Task DeleteMessageAsync(ulong id)
        {
            var restClient =
                new RestClient(
                    $"https://discord.com/api/webhooks/{Config.WebhookId}/{Config.WebhookToken}/messages/{id}");
            var req = new RestRequest(Method.DELETE);
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
            if (str.Length > 1995)
                str = str[..1995] + "(...)";
            return str;
        }

        public static string ToGoodString(this SocketMessage message)
            => message.Author.ToBetterString() + "> " + message.Content;

        public static string ReplaceRevoltMentions(this string str)
            => Program.ReplaceRevoltMentions.Replace(str, match =>
            {
                var id = match.Value[2..28];
                var mention = Program.Client.UsersCache.FirstOrDefault(u => u._id == id);
                if (mention != null)
                    return '@' + mention.Username;
                return match.Value;
            });

        public static string SizeToString(this Attachment att)
        {
            var size = (decimal) att.Size;
            if (size > 1_000_000)
                return Meth.Round(size / 1000 / 1000, 2) + "MB";
            if (size > 1_000)
                return Meth.Round(size / 1000, 2) + "KB";
            return size + "B";
        }
    }
}