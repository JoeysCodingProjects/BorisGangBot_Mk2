using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;

namespace BorisGangBot_Mk2.Modules
{
    [Name("Help Command")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _discord;

        public HelpModule(CommandService service,
            IConfigurationRoot config,
            DiscordSocketClient discord)
        {
            _service = service;
            _config = config;
            _discord = discord;
        }


        #region Help Command General
        [Command("help")]
        [Alias("h")]
        [Summary("Lists the available commands for Boris Gang Bot.")]
        public async Task HelpAsync()
        {
            string prefix = _config["prefix"];
            var author = new EmbedAuthorBuilder()
            {
                Name = _discord.CurrentUser.Username,
                IconUrl = _discord.CurrentUser.GetAvatarUrl()
            };
            var builder = new EmbedBuilder()
            {
                Title = "Below is a list of available commands:",
                Color = new Color(62, 33, 210),
                Author = author
                
            };

            foreach (var module in _service.Modules)
            {
                if (module.Group == "admin")
                        continue;
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"**{prefix}{cmd.Aliases.First()}**\nParameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" + $"{cmd.Summary}\n\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = $"\n\n```{module.Name}```";
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }
        #endregion

        #region Help Command Specific
        [Command("help")]
        [Alias("h")]
        [Summary("Add a command to the end to get more information about it.")]
        public async Task HelpAsync(string command)
        {
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Command **{command}** either was not found or does not exist.");
                return;
            }

            string prefix = _config["prefix"];
            var builder = new EmbedBuilder()
            {
                Color = new Color(138, 43, 226),
                Description = $"Here are some commands similar to **{command}**"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" + $"Summary: {cmd.Summary}";
                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
        #endregion
    }
}
