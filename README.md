# Reading YouTube Music Json
__THIS IS STILL A WORK IN PROGRESS: AS IN I WORKED ON IT WHEN I WAS HALF ASLEEP ONCE__
Basic idea of this program is to take in a Json file from Google Takeout showing all your YouTube Music and YouTube history and outputting something that can be put into listenbrainz.
It is currently reads the file from "music-history.json" just cause that's what I named it at the time, will eventually change to being a cli-argument.

I would rather keep this as a cli tool as to keep it cross platform as long as you have the dotnet runtime.

Currently works but only through 30 json files - will see how many songs that is

__TODO__
- Take in command-line arguments
- Implement functionality with ListenBrainz
- Publish a dll or "stable" release