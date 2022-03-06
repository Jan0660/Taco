---
sidebar_position: 1
slug: /
---

# Revolt.Net

[Check here for a quick example bot with commands set up.](https://github.com/Jan0660/RevoltBotExample)

## Minimal Example

This example connects via websocket and logs received messages.

```csharp
using Revolt; // Install Revolt.Net from NuGet 

var client = new RevoltClient();
await client.LoginAsync(TokenType.Bot, "");
client.OnReady += async () => { Console.WriteLine("Ready!"); };
client.MessageReceived += async msg => Console.WriteLine($"{msg.Author.Username}: {msg.Content}");
await client.ConnectWebSocketAsync();

// prevent the program exiting
await Task.Delay(-1);
```
