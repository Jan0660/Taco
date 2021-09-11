# Taco

## Setup
To deploy Taco you need to set up the `config.json` file by the following example:
```json
{
    "MongoUrl": "mongodb://user:password@host",
    "DatabaseName": "taco",
    "BotToken": ""
}
```

`MongoUrl` is the same format as you'd use in MongoDB Compass, make sure the database you created has the `users`, `servers` and `groups` collections.
Then just running `dotnet run` with the .NET 5 SDK will start the bot!

To give yourself Developer privileges on your instance of the bot add yourself to the `users` collection in MongoDB as:
```json
{
    "UserId": "your id",
    "PermissionLevel": 100
}
```
