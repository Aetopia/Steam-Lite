# Steam Lite
A minimal Steam Client frontend.

## Aim
This project started with the intention of replacing the entire Steam Client frontend with something similar to the UI of the Steam Client with the CEF disabled.
Steam Lite provides a minimal GUI frontend with the ability to launch games.

### How does program work?
This program wouldn't have been possible without [NoSteamWebHelper](https://github.com/Aetopia/NoSteamWebHelper).

1. The program will attempt to invoke `SteamClient.Launch()` and initialize a new Steam Client instance, if an instance of the Steam Client is running, it will be discarded/shutdown.
2. Once the new Steam Client instance is running, the Steam WebHelper is disabled to save on resources or to make it minimal.
3. To launch a game, the program calls `SteamClient.LaunchGameId(gameId)`, the method re-enables the Steam WebHelper, waits for the app to launch and then suspends the WebHelper again.
> [!NOTE]
> The frontend restricts the user to only launching a single app.
> Technically the `SteamClient` class, allows you to launch multiple apps but blocks the invoking thread until the app terminates.

## Developer Resources
Check out the Steam Lite Wiki for more documentation.

