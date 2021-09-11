## Discord & Revolt Bridge
A temporary Discord & Revolt bridge until first-party support is added.

Automatically makes sure @everyone and other pings dont actually ping.

`./config.json`:
```json
{
  "DiscordBotToken": "",
  "RevoltBotToken": "",
  "RevoltUserId": "",
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
