using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BorisGangBot_Mk2.Services.GuildInfo
{
    public class GuildInfoService : GuildInfoBase
    {
        private readonly DiscordSocketClient _discord;

        public GuildInfoService(DiscordSocketClient discord)
        {
            _discord = discord;

            Guilds = new Dictionary<ulong, SocketGuild>();
            GuildRoles = new Dictionary<ulong, Dictionary<string, SocketRole>>();

            _discord.GuildUnavailable += ClearGuildAsync;
            _discord.GuildUnavailable += ClearGuildRolesAsync;
            _discord.GuildAvailable += StoreGuildAsync;
            _discord.GuildAvailable += StoreGuildRolesAsync;

        }


        #region Primary functions for GuildInfo


        #region Store/Clear guilds 

        private Task StoreGuildAsync(SocketGuild guild)
        {
            if (guild == null)
                return Task.Run(() => Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh: mm:ss")} [Guild Info Service ERROR]: Guild returned NULL."));
            
            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [Guild Info Service]: Successfully saved info for the guild {guild.Name}");
            return Task.Run(() => Guilds.Add(guild.Id, guild));
        }

        private Task ClearGuildAsync(SocketGuild guild)
        {
            return Task.Run(() => Guilds.Remove(guild.Id));
        }

        #endregion

        #region Store/Clear Guild Roles

        private Task StoreGuildRolesAsync(SocketGuild guild)
        {
            IReadOnlyCollection<SocketRole> rolesCollection = guild.Roles;
            Dictionary<string, SocketRole> guildroles = CreateRoleDictionary(rolesCollection.GetEnumerator(), rolesCollection.Count);
            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [Guild Info Service]: Successfully saved roles for the guild {guild.Name}");
            return Task.Run(() => GuildRoles.Add(guild.Id, guildroles));
        }

        private Task ClearGuildRolesAsync(SocketGuild guild)
        {
            return Task.Run(() => GuildRoles.Remove(guild.Id));
        }

        #endregion


        #endregion


        #region Secondary functions for Guild Info (Supporting Functions)


        #region CreateRoleDictionary

        private static Dictionary<string, SocketRole> CreateRoleDictionary(IEnumerator<SocketRole> guildRoles, int count)
        {
            Dictionary<string, SocketRole> roles = new Dictionary<string, SocketRole>();
            IEnumerator<SocketRole> guildRolesPtr = guildRoles;
            guildRolesPtr.MoveNext();
            int i = 1;
            while (i < count + 1)
            {
                if (guildRolesPtr.Current != null)
                    roles.Add(guildRolesPtr.Current.Name.ToLower(), guildRolesPtr.Current);
                i++;
                guildRolesPtr.MoveNext();
            }

            return roles;
        }

        #endregion

        #endregion
    }
}
