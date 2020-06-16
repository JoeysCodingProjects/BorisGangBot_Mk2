[![CodeFactor](https://www.codefactor.io/repository/github/joeyscodingprojects/borisgangbot_mk2/badge/master)](https://www.codefactor.io/repository/github/joeyscodingprojects/borisgangbot_mk2/overview/master)
# BorisGangBot_Mk2

## Written in C# using Discord.Net and TwitchLib


## What is this?
BorisGangBot is a project of mine to create my own fully fleshed out bot for my friends and I's discord server.


## What are it's functions?
The bot is (or is going to be) capable of:
- [x] Twitch Stream alert notifications
- [ ] Music bot functionalities
- [x] Automatic role assignment through a "roleme {rolename}" esque function
- [ ] Quickly provide information about any twitch stream
- [ ] ...And much much more.


## Update : 6-16-2020
Bot is in a somewhat stable state. I believe I've fixed most of the
bugs inside Live Stream Monitor such as duplicate alerts, failing to find streamers, not updating
the streamers list, etc. Short term testing shows no issues so far, have currently testing long term. 
The code through out the program is still pretty inefficient and hard to read, but I'll be rewriting
a majority of the code once I get music playing functionality up and running.

## Notes on setting up the bot:
I've just pushed an update to make the bot very easy to just clone the repository and run, but you must follow this step first.
You'll need to create a \_config.yml file in the base directory (where StartUp.cs is) containing the following:
```yaml
prefix: ;
tokens:
    tw_cID: YourTwitchClientIdHere
    tw_token: YourTwitchTokenHere
    discord: YourDiscordBotTokenHere
    discord_testing: YourDebuggingDiscordBotTokenHere

botActivity:
    activity: listening/playing/watching/streaming
    description: ;h for help

liveStreamMono:
    updIntervalSeconds: 30
    notifChannelName: stream-updates
```
- Prefix - The prefix used for the bots commands, can be any combo of symbols and letters
- tw_cID - Your twitch client ID
- tw_token - Your twitch Access Token
- discord - Your discord bots token
- discord_testing - Optional. A token for a second bot used for testing features outside of the main server
- activity - Can be one of those 4 options, will default to listening
- descrition - What comes after the activity, for example: "Listening to ;h for help"
- updIntervalSeconds - How often the bot should check twitch to see if any streams have come online
- notifChannelName - Name of the Discord channel to send stream alerts into. Channel name only has to contain what you put for notifChannelName
so surrounding the name with emojis on discord if fine.

## Feel free to use any of the code you see!
It may be ugly code, but I'm sure it can offer plenty of assitance to anyone who may run into some awkward problems. If
you would like me to clarify on a block of code, how I did something, or what something does, please don't hesitate
to create an issue flagged as a **question** or even one asking that I return to a block of code and add
comments explaining what's going on. I'm happy to assist. This is a learning experience for me and I'd be glad
to share what I learn with others along the way.

Anyways, hope someone finds this useful. Took a lot of time to learn all this but should anyone stumble upon this code
looking for help in writing a discord bot in C#, it should come in handy a decent bit. Enjoy!
