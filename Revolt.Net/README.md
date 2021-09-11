# Revolt.Net

A .NET library for Revolt with both the sweetness of OOP and DOD.

This library is [Available on NuGet](https://www.nuget.org/packages/Revolt.Net/) but I'd recommend you use [git submodules](https://git-scm.com/book/en/v2/Git-Tools-Submodules) for the latest cutting-edge changes.

## Quickstart
```csharp
using Revolt;

client = new RevoltClient("bot token", "bot user id WILL BE REMOVED IN THE FUTURE");
await client.ConnectWebSocketAsync();
```
And now let intellisense guide you around :)
