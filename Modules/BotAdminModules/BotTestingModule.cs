using BorisGangBot_Mk2.Services;
using BorisGangBot_Mk2.Services.GuildInfo;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [Command("randominfo")]
        [Summary("I still use print statements to test my code because im a naughty boy")]
        public async Task SendRandomInfoAsync()
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            IEnumerable<string> userroles = user.Roles.Select(r => r.Name);
            string msg = "";
            foreach (string r in userroles)
            {
                msg += " " + r;
            }
            await ReplyAsync(msg);
        }
    }
}
