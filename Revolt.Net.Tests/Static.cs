using System.IO;
using System.Text.Json;

namespace Revolt.Net.Tests;

public static class Static
{
    public static RevoltClient Bot { get; private set; }
    public static RevoltClient User { get; private set; }
    static Static()
    {
        var config = JsonSerializer.Deserialize<Config>(File.ReadAllText("./config.json"));
        Bot = new();
        Bot.LoginAsync(TokenType.Bot, config!.BotToken).Wait();
        User = new();
        User.LoginAsync(TokenType.User, config!.UserToken).Wait();
    }
    private class Config
    {
        public string BotToken { get; set; }
        public string UserToken { get; set; }
    }
}