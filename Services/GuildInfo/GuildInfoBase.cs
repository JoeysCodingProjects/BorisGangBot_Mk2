using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;

namespace BorisGangBot_Mk2.Services.GuildInfo
{
    public abstract class GuildInfoBase
    {
        //
        //
        // GuildInfo variables
        // All Dictionary info stored with related guild's
        // id stored as a ulong
        //
        //

        public List<string> RoleMeEnabledRoles { get; set; }

        public Dictionary<ulong, SocketGuild> Guilds { get; set; }

        public Dictionary<ulong, Dictionary<string, SocketRole>> GuildRoles { get; set; }

    }
}
