using BorisGangBot_Mk2.Helpers;
using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace BorisGangBot_Mk2.Services
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _provider;

        public CommandHandler(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            IServiceProvider provider)
        {
            _discord = discord;
            _commands = commands;
            _config = config;
            _provider = provider;

            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage s)
        {
            var msg = s as SocketUserMessage; // Ensure message is from user/bot
            if (msg == null)
                return;
            if (msg.Author.Id == _discord.CurrentUser.Id)
                return;
            if (msg.Author.IsBot)
                return;

            var context = new SocketCommandContext(_discord, msg); // Create the command context

            int argPos = 0; //Check for valid command prefix
            if (msg.HasStringPrefix(_config["prefix"], ref argPos) || msg.HasMentionPrefix(_discord.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _provider); // Execute command

                if (!result.IsSuccess)
                {
                    if (result.ErrorReason.ToString().Equals("Unknown command."))
                    {
                        await context.Channel.SendMessageAsync("I don't know that command, sorry.");
                    }
                    else
                    {
                        MessageOwnerHelper moh = new MessageOwnerHelper(_discord);
                        await moh.MessageOwnerAsync(msg, result.ErrorReason.ToString());
                        await context.Channel.SendMessageAsync(result.ToString());
                    }
                }
            }
        }
    }
}
