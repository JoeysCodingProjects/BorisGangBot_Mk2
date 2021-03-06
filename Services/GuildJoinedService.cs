﻿using Discord.WebSocket;
using System.Threading.Tasks;

namespace BorisGangBot_Mk2.Services
{
    public class GuildJoinedService
    {
        private readonly DiscordSocketClient _discord;

        public GuildJoinedService(DiscordSocketClient discord)
        {
            _discord = discord;

            _discord.JoinedGuild += OnJoinedGuildAsync;
        }

        private Task OnJoinedGuildAsync(SocketGuild guild)
        {

            return guild.DefaultChannel.SendMessageAsync("Thwanks for invwiting me here UwU, pwease send **;help** for a list of comwands my mwasta has given me UwU", false);
        }
    }
}
