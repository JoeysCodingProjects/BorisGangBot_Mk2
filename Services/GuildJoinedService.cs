using System;
using System.Text;
using System.Threading.Tasks;
using BorisGangBot_Mk2.Models;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace BorisGangBot_Mk2.Services
{
    public class GuildJoinedService
    {
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _provider;

        public GuildJoinedService(
            DiscordSocketClient discord,
            IServiceProvider provider)
        {
            _discord = discord;
            _provider = provider;

            _discord.JoinedGuild += OnJoinedGuildAsync;
        }

        private Task OnJoinedGuildAsync(SocketGuild guild)
        {

            return guild.DefaultChannel.SendMessageAsync("Thwanks for invwiting me here UwU, pwease send **;help** for a list of comwands my mwasta has given me UwU", false);
        }
    }
}
