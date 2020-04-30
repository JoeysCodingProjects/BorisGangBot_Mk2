using System;
using System.Reflection;
using System.Threading.Tasks;
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

        public StartUpService(
            IServiceProvider provider,
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            StreamMonoService streams)
        {
            _provider = provider;
            _discord = discord;
            _commands = commands;
            _config = config;
            _streams = streams;
        }

        public async Task StartAsync()
        {
            string discordToken = _config["tokens:discord"]; // Get the discord token from config
            if (string.IsNullOrWhiteSpace(discordToken))
            {
                throw new Exception("Please enter your bot's token into the '_configuration.json' file");
            }

            await _discord.LoginAsync(TokenType.Bot, discordToken);
            await _discord.StartAsync();

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider); // Load commands and modules into the command service

            _streams.UpdInt = 60; // Number of seconds between StreamMonoService updates
            _streams.StartStreamMonoAsync();
        }
    }
}
