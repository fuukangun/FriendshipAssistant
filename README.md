# FriendshipAssistant

FriendshipAssistant is a Stardew Valley / SMAPI mod scaffold for post-dialogue gift suggestions.

## Build

The project targets .NET 6 and expects a local Stardew Valley + SMAPI install. On macOS it defaults to:

`$HOME/Library/Application Support/Steam/steamapps/common/Stardew Valley/Contents/MacOS`

Override the path if needed:

```bash
dotnet build FriendshipAssistant.csproj -c Release -p:GamePath="/path/to/Stardew Valley/Contents/MacOS"
```

Run unit tests:

```bash
dotnet test FriendshipAssistant.Tests.csproj
```
