# Reading YouTube Music Json
Basic idea of this program is to take in a Json file from Google Takeout showing all your YouTube Music and YouTube history and outputting something that can be put into listenbrainz.
It is currently reads the file from "music-history.json" just cause that's what I named it at the time, will eventually change to being a cli-argument.

I would rather keep this as a cli tool as to keep it cross platform as long as you have the dotnet runtime.

Can work on a library that is however large, but it will take a while as there is a max limit to how many songs can be uploaded in one session.
So instead of doing just one very long json file that gets uploaded, several are used and uploaded.
Each json file is about 10200 bytes in size according to their [specifications](https://listenbrainz.readthedocs.io/en/v-2021-12-21.0/dev/api/#listenbrainz.webserver.views.api_tools.MAX_LISTEN_SIZE).

Now that it is able to properly communicate with ListenBrainz, the next major step is to take in cli arguments and make the music-uploads optional if you have never uploaded music.

__TODO__
- Take in command-line arguments
- Publish a dll or "stable" release
- Interact with YouTube API (Skip all the Google Takeout wait)
