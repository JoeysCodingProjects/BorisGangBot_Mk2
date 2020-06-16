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
Bot (in branch Exceps+Fixes) is in a somewhat stable state. I believe I've fixed most of the
bugs inside Live Stream Monitor such as duplicate alerts, failing to find streamers, not updating
the streamers list, etc. The code through out the program is still pretty inefficient and hard to
read, but I'll be coming back to that after I finish up cleaning up the bugs.

## Notes on setting up the bot:
If you were to simply reuse the entire repository, you'll need to create a \_config.yml file in the base directory (where StartUp.cs is) containing the following:
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
I apologize for the lack of comments, still working on that. Also, I hope you can ignore
all the inside jokes or weirdly named variables. Please feel free to ask any questions 
by submitting an Issue (pretty good way to get in touch with me) or by some other means
you can think of assuming you can get my info.


Anyways, hope someone finds this useful. Took a lot of time to learn all this but should anyone stumble upon this code
looking for help in writing a discord bot in C#, it should come in handy a decent bit. Enjoy!
