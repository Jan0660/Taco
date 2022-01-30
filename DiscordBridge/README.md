## Discord & Revolt Bridge
A temporary Discord & Revolt bridge until first-party support is added.

Automatically makes sure @everyone and other pings dont actually ping.

`./config.json`:
```json
{
  "DiscordBotToken": "",
  "RevoltBotToken": "",
  // optional, send across Revolt system messages(joins, leaves, kicks, bans, etc.)(default: true)
  // "RevoltSystemMessages": false,
  // optional, send across Discord system messages(joins, boosts)(default: true)
  // "DiscordSystemMessages": false,
  // optional, disable fix for Revolt quote format(default: true
  // "RevoltQuoteFix": false,
  "Channels": [
    {
      "DiscordChannelId": "",
      "RevoltChannelId": "",
      // optional, leave these empty or null and a new webhook will be created or reused automatically(Discord bot account must have the "Manage Webhooks" permission)
      "WebhookId": "",
      "WebhookToken": ""
    }
    // since this is json, for other properties, use a comma and repeat the object before like so:
    /*
    ,
    {
      "DiscordChannelId": "",
      <other properties>
    }
    */
  ]
}
```
(Note: JSON with comments will be parsed but then overwritten)

# Build & Run (Linux)

1. [Install .NET 6](https://docs.microsoft.com/en-us/dotnet/core/install/linux) (or your distribution-provided instructions).
2. Git clone the repository.
3. Build using: `dotnet publish DiscordBridge -r linux-x64 -c release`
4. Ensure the Revolt bot has the `Masquerade` permission.
5. Ensure the Discord bot has the Server Members and Message Content Privileged Intents enabled in the Dev Portal.
6. Run using: `./DiscordBridge/bin/release/net6.0/linux-x64/publish/DiscordBridge`

# Docker

A Dockerfile and docker-compose.yml is included in the project root.

Or, if you hate yourself, you can use Docker CLI:

```bash
docker build -f DiscordBridge/Dockerfile -t discord-bridge .
docker run -v $PWD/config.json:/app/config.json discord-bridge
```
