using BorisGangBot_Mk2.Services;
using BorisGangBot_Mk2.Services.GuildInfo;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BorisGangBot_Mk2.Services.LiveStreamMono;

namespace BorisGangBot_Mk2.Modules.BotAdminModules
{
    [RequireOwner]
    [RequireUserPermission(Discord.GuildPermission.Administrator)]
    [Group("admin")]
    public class BotTestingModule : ModuleBase<SocketCommandContext>
    {
        private IConfigurationRoot _config;
        private StreamMonoService _streams;
        private GuildInfoService _guildinfo;

        public BotTestingModule(IConfigurationRoot config,
            StreamMonoService streams,
            GuildInfoService guildinfo)
        {
            _config = config;
            _streams = streams;
            _guildinfo = guildinfo;
        }

        [Command("test")]
        [Summary("I still use print statements to test my code because im a naughty boy")]
        public async Task SendRandomInfoAsync()
        {
            string msg = "";
            foreach (string r in _streams.StreamIdList)
            {
                msg += " " + r;
            }
            await ReplyAsync(msg);
        }
    }
}
