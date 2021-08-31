using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Discord;
using Discord.Webhook;
using Discord.WebSocket;
using Log73;
using Newtonsoft.Json;
using Revolt;
using Revolt.Channels;
using Attachment = Revolt.Attachment;
using Console = Log73.Console;
using Meth = System.Math;

namespace DiscordBridge
{
    class Program
    {
        private static RevoltClient _client;
        public static RevoltClient Client => _client;
        public static DiscordSocketClient DiscordClient;
        public static Config Config;
        public static readonly Dictionary<string, (ulong MessageId, ulong ChannelId)> RevoltDiscordMessages = new();
        public static readonly Dictionary<ulong, string> DiscordRevoltMessages = new();
        public static readonly List<string> DiscordRevoltMessagesContent = new();
        public static readonly Regex ReplaceRevoltMentions = new("<@([0-9,A-Z]{26})+>", RegexOptions.Compiled);
        public static readonly Regex ReplaceDiscordMentions = new("<@!?[0-9]{1,20}>", RegexOptions.Compiled);
        public static readonly Regex ReplaceDiscordEmotes = new("<a?:.+?:[0-9]{1,20}>", RegexOptions.Compiled);

        static async Task Main()
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
            Console.Info("Creating Discord webhook clients...");
            foreach (var channel in Config.Channels)
            {
                try
                {
                    channel.DiscordWebhook = new DiscordWebhookClient(channel.WebhookId, channel.WebhookToken);
                }
                catch
                {
                    // shut up
                }
            }

            _client.MessageReceived += async message =>
            {
                var channel = Config.ByRevoltId(message.ChannelId);
                if (channel == null)
                    return;
                if (message.AuthorId == Config.RevoltSession.UserId && DiscordRevoltMessagesContent.Contains(message.Content))
                    return;
                try
                {
                    var embeds = new List<Embed>();
                    ulong replyToId = RevoltDiscordMessages
                        .FirstOrDefault(m => m.Key == message.Replies?.FirstOrDefault()).Value.MessageId;
                    if (replyToId == 0)
                        replyToId = DiscordRevoltMessages
                            .FirstOrDefault(m => m.Value == message.Replies?.FirstOrDefault()).Key;
                    Func<Task> updateReply = null;
                    if (replyToId != 0)
                    {
                        var index = embeds.Count;
                        embeds.Add(new EmbedBuilder()
                        {
                            Title = "Quoted Message",
                            Url =
                                $"https://discord.com/channels/{channel.DiscordServerId}/{channel.DiscordChannelId}/{replyToId}",
                            Color = new Color(47, 49, 54),
                        }.Build());
                        updateReply = async () =>
                        {
                            var msg = await DiscordClient.GetGuild(channel.DiscordServerId)
                                .GetTextChannel(channel.DiscordChannelId)
                                .GetMessageAsync(replyToId);
                            embeds[index] = new EmbedBuilder()
                            {
                                Author = new()
                                {
                                    Name = msg.Author.ToString(),
                                    IconUrl = msg.Author.GetAvatarUrl() ?? msg.Author.GetDefaultAvatarUrl(),
                                    Url =
                                        $"https://discord.com/channels/{channel.DiscordServerId}/{channel.DiscordChannelId}/{replyToId}",
                                },
                                Color = new Color(47, 49, 54),
                                Description = msg.Content
                            }.Build();
                            await channel.DiscordWebhook.ModifyMessageAsync(
                                RevoltDiscordMessages[message._id].MessageId, x => x.Embeds = embeds);
                        };
                    }

                    if (message.Attachments != null)
                        foreach (var attachment in message.Attachments)
                        {
                            embeds.Add(new EmbedBuilder()
                            {
                                Title = attachment.Filename,
                                ImageUrl =
                                    $"https://autumn.revolt.chat/attachments/{attachment._id}/{HttpUtility.UrlEncode(attachment.Filename)}",
                                Color = new Color(47, 49, 54),
                                Footer = new()
                                {
                                    Text =
                                        $"{attachment.SizeToString()} • {attachment.Metadata.Width}x{attachment.Metadata.Height}"
                                },
                                Url =
                                    $"https://autumn.revolt.chat/attachments/{attachment._id}/{HttpUtility.UrlEncode(attachment.Filename)}"
                            }.Build());
                        }

                    var msg = await channel.DiscordWebhook.SendMessageAsync(message.Content.ReplaceRevoltMentions(),
                        username: message.Author.Username,
                        avatarUrl: message.Author.Avatar == null
                            ? message.Author.DefaultAvatarUrl
                            : message.Author.AvatarUrl + "?size=256", allowedMentions: new(), embeds: embeds);
                    RevoltDiscordMessages.Add(message._id, (msg, channel.DiscordChannelId));
                    if (updateReply != null)
                        await updateReply();
                }
                catch (Exception exc)
                {
                    Console.Exception(exc);
                }
            };
            _client.MessageUpdated += (id, data) =>
            {
                if (RevoltDiscordMessages.TryGetValue(id, out var discordSide))
                {
                    return Config.ByDiscordId(discordSide.ChannelId).DiscordWebhook.ModifyMessageAsync(
                        discordSide.MessageId,
                        c => c.Content = data.Content.ReplaceRevoltMentions());
                }

                return Task.CompletedTask;
            };
            _client.MessageDeleted += (messageId) =>
            {
                if (RevoltDiscordMessages.TryGetValue(messageId, out var discordSide))
                {
                    return Config.ByDiscordId(discordSide.ChannelId).DiscordWebhook
                        .DeleteMessageAsync(discordSide.MessageId);
                }

                return Task.CompletedTask;
            };
            _client.SystemMessageReceived += msg =>
            {
                var channel = Config.ByRevoltId(msg.ChannelId);
                if (channel == null)
                    return Task.CompletedTask;
                if (msg.Channel is not GroupChannel group)
                    return Task.CompletedTask;
                return channel.DiscordWebhook.SendMessageAsync(msg.Stringify(), allowedMentions: new AllowedMentions(),
                    avatarUrl: (msg.Content.Id == null ? null : Client.Users.Get(msg.Content.Id).AvatarUrl) ??
                               group.Icon?.GetUrl() ?? "https://app.revolt.chat/assets/group.png",
                    username: "Revolt Bridge");
            };
            DiscordClient = new DiscordSocketClient();
            await DiscordClient.LoginAsync(TokenType.Bot, Config.DiscordBotToken);
            await DiscordClient.StartAsync();
            DiscordClient.Ready += async () =>
            {
                Console.Log("Discord ready!");
                foreach (var channel in Config.Channels)
                {
                    var discordChannel = DiscordClient.GetChannel(channel.DiscordChannelId);
                    if (channel.DiscordServerId == 0)
                        channel.DiscordServerId = (discordChannel as SocketGuildChannel)?.Guild?.Id ?? 0;
                    if (discordChannel is SocketCategoryChannel category)
                    {
                        foreach (var h in category.Channels)
                        {
                            if (channel.RevoltServerId != null && h is SocketTextChannel textH &&
                                Config.ByDiscordId(h.Id) == null)
                            {
                                var wh = await textH.CreateWebhookAsync("REVOLT Bridge");
                                var ch = await _client.Servers.CreateChannel(channel.RevoltServerId, h.Name,
                                    $"Bridged from Discord(Id: {h.Id})");
                                Config.Channels.Add(new BridgeChannel
                                {
                                    RevoltChannelId = ch._id,
                                    WebhookId = wh.Id,
                                    WebhookToken = wh.Token,
                                    DiscordChannelId = h.Id,
                                    DiscordWebhook = new(wh.Id, wh.Token)
                                });
                            }
                        }
                    }
                }

                File.WriteAllText("./config.json", JsonConvert.SerializeObject(Config, Formatting.Indented));
            };
            DiscordClient.MessageReceived += async message =>
            {
                var channel = Config.ByDiscordId(message.Channel.Id);
                if (channel == null)
                    return;
                if (message.Author.Id == channel.WebhookId)
                    return;
                if (message.Content == "-uber" && message.Author.IsBot == false)
                {
                    await message.Channel.SendFileAsync("../Taco/Resources/UberFruit.png");
                }

                if (message.Content == "-bridge-status" && message.Author.IsBot == false)
                {
                    await message.Channel.SendMessageAsync($@"**DiscordRevoltMessages:** {DiscordRevoltMessages.Count}
**RevoltDiscordMessages:** {RevoltDiscordMessages.Count}
**Discord Latency:** {DiscordClient.Latency}");
                }

                try
                {
                    MessageReply reply = new();
                    bool isReply = false;
                    if (message.Reference != null)
                    {
                        try
                        {
                            string replyToId = null;
                            if (DiscordRevoltMessages.TryGetValue(message.Reference.MessageId.Value, out var msgId))
                                replyToId = msgId;
                            var h = RevoltDiscordMessages.FirstOrDefault(m =>
                                m.Value.MessageId == message.Reference.MessageId.Value);
                            if (h.Key != null)
                                replyToId = h.Key;
                            if (replyToId == null)
                                throw new Exception("Didn't find message to reply to.");
                            isReply = true;
                            reply = new()
                            {
                                Id = replyToId,
                                Mention = true
                            };
                        }
                        catch
                        {
                            isReply = false;
                        }
                    }

                    Message msg;
                    var content = message.ToGoodString();
                    DiscordRevoltMessagesContent.LimitedAdd(content, 50);
                    if (message.Attachments.Any())
                    {
                        var http = new HttpClient();
                        var attachment = await http.GetByteArrayAsync(message.Attachments.First().ProxyUrl);
                        var attachmentId = await _client.UploadFile(message.Attachments.First().Filename, attachment);
                        msg = await _client.Channels.SendMessageAsync(
                            channel.RevoltChannelId, content,
                            new() { attachmentId }, isReply ? new[] { reply } : null);
                    }
                    else
                        msg = await _client.Channels.SendMessageAsync(channel.RevoltChannelId,
                            content, replies: isReply ? new[] { reply } : null);

                    if(msg._id != null)
                        DiscordRevoltMessages.Add(message.Id, msg._id);
                }
                catch (Exception exc)
                {
                    Console.Exception(exc);
                }
            };
            DiscordClient.MessageUpdated += async (cacheable, message, channel) =>
            {
                if (message.Content == null)
                    return;
                if (DiscordRevoltMessages.TryGetValue(cacheable.Id, out string revoltMessageId))
                {
                    await _client.Channels.EditMessageAsync(Config.ByDiscordId(channel.Id).RevoltChannelId,
                        revoltMessageId,
                        message.ToGoodString());
                }
            };
            DiscordClient.MessageDeleted += (cacheable, channel) =>
            {
                if (DiscordRevoltMessages.TryGetValue(cacheable.Id, out string id))
                {
                    return _client.Channels.DeleteMessageAsync(Config.ByDiscordId(channel.Id).RevoltChannelId, id);
                }

                return Task.CompletedTask;
            };
            await Task.Delay(-1);
        }
    }

    public class Config
    {
        public List<BridgeChannel> Channels;
        public string DiscordBotToken;
        public Session RevoltSession;

        public BridgeChannel ByDiscordId(ulong id)
            => Channels.FirstOrDefault(c => c.DiscordChannelId == id);

        public BridgeChannel ByRevoltId(string id)
            => Channels.FirstOrDefault(c => c.RevoltChannelId == id);
    }

    public class BridgeChannel
    {
        public ulong WebhookId;
        public string WebhookToken;
        public ulong DiscordChannelId;
        public ulong DiscordServerId;
        public string RevoltChannelId;
        public string? RevoltServerId;
        [JsonIgnore] public DiscordWebhookClient DiscordWebhook;
    }

    public static class ExtensionMethods
    {
        [Pure]
        public static string Shorten(this string str, int length)
            => str.Length > length ? str[..(length - 5)] + "(...)" : str;

        public static string ToBetterString(this SocketUser user)
        {
            var str = user.Discriminator == "0000" ? user.Username : user.ToString();
            if (str.StartsWith("# "))
                str = '\\' + str;
            str = str.Replace("$", "\\$");
            str = str.Shorten(1999);
            return str;
        }

        public static string ReplaceDiscordMentions(this string str)
            => Program.ReplaceDiscordMentions.Replace(str,
                match =>
                {
                    ulong id = UInt64.Parse(
                        match.Value[2..^1].StartsWith('!') ? match.Value[3..^1] : match.Value[2..^1]);
                    var user = Program.DiscordClient.GetUser(id);
                    if (user == null)
                        return match.Value;
                    return $"@{user}";
                });

        public static string ToGoodString(this SocketMessage message)
        {
            string str = "";
            var content = Program.ReplaceDiscordEmotes.Replace(message.Content, match =>
            {
                int startIndex = 0, endIndex = match.Value.Length - 1;
                while (match.Value[startIndex] != ':')
                    startIndex++;
                while (match.Value[endIndex] != ':')
                    endIndex--;
                return match.Value[startIndex..(endIndex + 1)];
            });
            str += message.Author.ToBetterString() + "> " + content.ReplaceDiscordMentions();

            return str.Shorten(2000);
        }

        public static string ReplaceRevoltMentions(this string str)
            => Program.ReplaceRevoltMentions.Replace(str, match =>
            {
                var id = match.Value[2..28];
                var mention = Program.Client.UsersCache.FirstOrDefault(u => u._id == id);
                if (mention != null)
                    return '@' + mention.Username.Replace("_", "\\_");
                return match.Value;
            });

        public static string SizeToString(this Attachment att)
        {
            var size = (decimal)att.Size;
            if (size > 1_000_000)
                return Meth.Round(size / 1000 / 1000, 2) + "MB";
            if (size > 1_000)
                return Meth.Round(size / 1000, 2) + "KB";
            return size + "B";
        }

        public static string GetUrl(this Attachment attachment)
            =>
                $"{Program.Client.ApiInfo.Features.Autumn.Url}/{attachment.Tag}/{attachment._id}/{HttpUtility.UrlEncode(attachment.Filename)}";

        public static string Stringify(this ObjectMessage msg)
        {
            string GetUsername(string id)
                => $"**@{Program.Client.Users.Get(id)?.Username ?? "(Unknown user)"}**";

            return msg.Content.Type switch
            {
                "text" => msg.Content.Content,
                "user_added" =>
                    $"{GetUsername(msg.Content.Id)} was added to the group by {GetUsername(msg.Content.By)}.",
                "user_remove" =>
                    $"{GetUsername(msg.Content.Id)} was removed from the group by {GetUsername(msg.Content.By)}.",
                "user_joined" => $"{GetUsername(msg.Content.Id)} has joined the server.",
                "user_left" => $"{GetUsername(msg.Content.Id)} has left the group.",
                "user_kicked" => $"{GetUsername(msg.Content.Id)} was kicked from the server.",
                "user_banned" => $"{GetUsername(msg.Content.Id)} was banned from the server.",
                "channel_renamed" => $"{GetUsername(msg.Content.By)} has renamed the channel to `{msg.Content.Name}`.",
                "channel_description_changed" => $"{GetUsername(msg.Content.By!)} changed the group description.",
                "channel_icon_changed" => $"{GetUsername(msg.Content.By)} has changed the channel icon.",
                _ => msg.Content.Type
            };
        }

        public static void LimitedAdd<T>(this List<T> list, T item, int maxCount)
        {
            list.Add(item);
            if (list.Count == maxCount)
                list.Remove(list.FirstOrDefault());
        }
    }
}