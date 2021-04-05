using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RevoltApi;

namespace Saltus
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client =
                new RevoltClient(JsonConvert.DeserializeObject<Session>(await File.ReadAllTextAsync("./session.json")));
            await client.ConnectWebSocketAsync();
            client.MessageReceived += AutoSalty;
            await Task.Delay(-1);
        }
        private static Task AutoSalty(Message msg)
        {
            if (msg.AuthorId == "01EXAG0ZFX02W7PNQE7W5MT339")
            {
                return msg.Channel.SendMessageAsync($@"> {msg.Content.Replace("\n", "\n> ")}

salty");
            }
            return Task.CompletedTask;
        }
    }
}