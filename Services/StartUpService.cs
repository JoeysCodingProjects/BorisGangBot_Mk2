using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using BorisGangBot_Mk2.Services.GuildInfo;
using BorisGangBot_Mk2.Services.LiveStreamMono;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace BorisGangBot_Mk2.Services
{
    public class StartUpService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly StreamMonoService _streams;
        private readonly GuildInfoService _guildinfo;

        public StartUpService(
            IServiceProvider provider,
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            StreamMonoService streams,
            GuildInfoService guildinfo)
        {
            _provider = provider;
            _discord = discord;
            _commands = commands;
            _config = config;
            _streams = streams;
            _guildinfo = guildinfo;
        }

        public async Task StartAsync()
        {
            string discordToken = _config["tokens:discord"]; // Token used by Released bot
            //string discordToken = _config["tokens:discord_testing"]; // Token used by separate dev version of bot

            if (string.IsNullOrWhiteSpace(discordToken))
            {
                throw new Exception("Please enter bot token into the '_config.yaml' file");
            }

            _discord.Ready += _streams.CreateStreamMonoAsync;

            await _discord.LoginAsync(TokenType.Bot, discordToken);
            await _discord.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider); // Load commands and modules into the command service

            if (_config["botActivity:activity"] != null)
            {
                switch (_config["botActivity:activity"].ToLower())
                {
                    case "listening":
                        await _discord.SetGameAsync($"{_config["botActivity:description"]}", null, ActivityType.Listening);
                        break;

                    case "playing":
                        await _discord.SetGameAsync($"{_config["botActivity:description"]}", null, ActivityType.Playing);
                        break;

                    case "watching":
                        await _discord.SetGameAsync($"{_config["botActivity:description"]}", null, ActivityType.Watching);
                        break;

                    case "streaming":
                        await _discord.SetGameAsync($"{_config["botActivity:description"]}", null, ActivityType.Streaming);
                        break;

                    default:
                        await Console.Out.WriteLineAsync($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StartUp]: Bot Activity value was misspelled or does not exist. Please double check _config.yml.");
                        break;
                }
            }
        }
    }
}
