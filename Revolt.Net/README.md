# Revolt.Net

A .NET library for Revolt with both the sweetness of OOP and DOD.

[Available on NuGet](https://www.nuget.org/packages/Revolt.Net/)

## Quickstart
```csharp
using Revolt;

var client = new RevoltClient(new Session(){
    Id = "",
    UserId = "",
    SessionToken = ""
});

await client.ConnectWebSocketAsync();
// client.MessageReceived
```
And now let intellisense guide you around :)
