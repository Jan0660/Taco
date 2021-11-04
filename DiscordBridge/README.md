## Discord & Revolt Bridge
A temporary Discord & Revolt bridge until first-party support is added.

Automatically makes sure @everyone and other pings dont actually ping.

`./config.json`:
```json
{
  "DiscordBotToken": "",
  "RevoltBotToken": "",
  "Channels": [
    {
      "WebhookId": "",
      "WebhookToken": "",
      "DiscordChannelId": "",
      "RevoltChannelId": ""
    }
  ]
}
```

# Build & Run (Linux)

1. [Install .NET 5](https://docs.microsoft.com/en-us/dotnet/core/install/linux) (or your distribution-provided instructions).
2. Git clone the repository.
3. Build using: `dotnet publish DiscordBridge -r linux-x64 -c release`
4. Ensure the Revolt bot has the `Masquerade` permission.
5. Run using: `./DiscordBridge/bin/release/net5.0/linux-x64/publish/DiscordBridge`


