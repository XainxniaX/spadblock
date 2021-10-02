# Spotify Ad Block / Mute 
This program mutes your computer/device when it detects that spotify (free) is playing an Ad. It uses a library SpotifyAPI-NET (https://github.com/JohnnyCrazy/SpotifyAPI-NET)
to interact with the spotify Web API. Then it uses scripts specific to each platform to mute and unmute the audio device(s). 

# Platforms
- Linux (ubuntu/mint) : requires "amixer" avalible in bash.
- Windows (Vista or newer) : uses powershell/.NET from https://stackoverflow.com/questions/21355891/change-audio-level-from-powershell
- Android : Not yet implemented, but Xamarin should allow audio manipulation.

# Spotify web API Authentication
The user will be prompted to visit https://developer.spotify.com/console/get-users-currently-playing-track/?market=US&additional_types=
in order to generate a token which allows this program to view the currently playing track/ad. In the future the user will instead sign in to spotify
and provide a permanent token-access to the web API. However this process works for now.
