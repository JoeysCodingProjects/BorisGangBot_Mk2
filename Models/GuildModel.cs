using System;
using System.Collections.Generic;
using System.Text;

namespace BorisGangBot_Mk2.Models
{
    public class GuildModel
    {
        public GuildModel(string _guildname, ulong _guildId, string _prefix, ulong _greetingChannelId)
        {
            GuildName = _guildname;
            GuildID = _guildId;
            Prefix = _prefix;
            GreetingChannelID = _greetingChannelId;
        }

        public string GuildName { get; set; }
        public ulong GuildID { get; set; }
        public string Prefix { get; set; }
        public ulong GreetingChannelID { get; set; }
    }
}
