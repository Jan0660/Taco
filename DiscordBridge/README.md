## Discord & Revolt Bridge
A temporary Discord & Revolt bridge until first-party support is added.

Automatically makes sure @everyone and other pings dont occur.

`./config.json`:
```json
{
  "WebhookId": "",
  "WebhookToken": "",
  "DiscordBotToken": "",
  "DiscordChannelId": "",
  "RevoltChannelId": "",
  "RevoltSession": {
    "id": "",
    "user_id": "",
    "session_token": ""
  }
}
```
(multi-channel support coming soon?)