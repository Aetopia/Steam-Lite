# Steam Lite
A minimal Steam Client frontend.

## Aim
This project started with the intention of replacing the entire Steam Client frontend with something similar to the UI of the Steam Client with the CEF disabled.
Steam Lite provides a minimal GUI frontend with the ability to launch games.

### How does program work?
This program wouldn't have been possible without [NoSteamWebHelper](https://github.com/Aetopia/NoSteamWebHelper).

1. The program will attempt to invoke `SteamClient.Launch()` and initialize a new Steam Client instance, if an instance of the Steam Client is running, it will be discarded/shutdown.
2. Once the new Steam Client instance is running, the Steam WebHelper is disabled to save on resources or to make the instance "minimal".
3. To launch a game, the program calls `SteamClient.LaunchGameId(gameId)`, the method re-enables the Steam WebHelper, waits for the app to launch and then suspends the WebHelper again.
> [!NOTE]
> The frontend restricts the user to only launching a single app.<br>
> Technically the `SteamClient` class, allows you to launch multiple apps but blocks the invoking thread until the app terminates.

### [`SteamClient` class](https://github.com/Aetopia/Steam-Lite/blob/main/SteamClient.cs)
The `SteamClient` provides methods for interacting with a Steam Client instance.

#### Methods
##### `SteamClient.Launch()`
Initialize a Steam Client instance and if required shutdown/discard any running Steam Client instance that wasn't invoked by this method.<br>
The method may fail if Steam isn't installed or an instance invoked by this is already running.<br>
The method returns a `Process` class object if a Steam Client instance is created else `null`.

##### `SteamClient.Shutdown()`
Shutdown any instance invoked by `SteamClient.Launch()`.<br>
The method returns true if a Steam Client instance invoked by `SteamClient.Launch()` is running else `false`.

##### `SteamClient.GetInstance()`
Obtain an already running Steam Client instance invoked by `SteamClient.Launch()`.<br>
The method returns `Process` class object if a Steam Client instance invoked by `SteamClient.Launch()` is running else `null`.

##### `SteamClient.GetApps()`
Obtains installed Steam applications with their App ID and name.<br>
This methods returns a dictionary contains the App IDs and names.<br>
The key-value pair is present as follows `{App ID: Name}`.

##### `SteamClient.WebHelper(bool enable)`
Disables or enables the Steam WebHelper for a Steam Client instance.
|Value of `enable`|Effect|
|-|-|
|`true`| Steam WebHelper is enabled.|
|`false`| Steam WebHelper is disabled.|

The method returns `true` if a Steam Client instance invoked by `SteamClient.Launch()` is running else `false`.

##### `SteamClient.StartGameId(string gameId)`
Runs the specified App ID.<br>
The method will block the invoking thread until the the app is terminated, so its best to use this method in a thread of its own.<br>
The method returns `true` if a Steam Client instance invoked by `SteamClient.Launch()` is running else `false`.

# Building
1. Install the follwing:<br>
    - [.NET SDK](https://dotnet.microsoft.com/en-us/download/visual-studio-sdks)
    - [.NET Framework 4.8.1 Developer Pack](https://go.microsoft.com/fwlink/?linkid=2203306)
2. Run the following commands in the repository's root directory to build the project:<br>
    ```
    dotnet restore
    dotnet clean
    dotnet build --configuration Release
    ```