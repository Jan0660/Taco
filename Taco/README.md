# Taco

## Setup
To setup Taco you need to create these files in this directory with their contents filled out.
To get your session data you can follow [this article](https://rvf.infi.sh/posts/Getting-Session-Data) by Infi.
`MongoUrl` is the same format as you'd use in MongoDB Compass, make sure the database you created has a `users` collection.

`session.json`:
```json
{
	"id": "",
	"user_id": "",
	"session_token": ""
}
```
`config.json`:
```json
{
    "MongoUrl": "mongodb://user:password@host",
    "DatabaseName": "taco"
}
```
Then just running `dotnet run` with the .NET 5 SDK will start the bot!

To give yourself Developer privileges on your instance of the bot add yourself to the `users` collection in MongoDB as:
```json
{
    "UserId": "your id",
    "PermissionLevel": 100
}
```
