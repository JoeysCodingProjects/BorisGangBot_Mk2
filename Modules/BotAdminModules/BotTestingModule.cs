using BorisGangBot_Mk2.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using System.IO;
using BorisGangBot_Mk2.Services;
using BorisGangBot_Mk2.Services.GuildInfo;

namespace BorisGangBot_Mk2.Modules.BotAdminModules
{
    [RequireOwner]
    public class BotTestingModule:ModuleBase<SocketCommandContext>
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
        public async Task SendRandomInfoAsync(string role)
        {
            SocketRole rolo;
            Dictionary<string, SocketRole> roledict;
            _guildinfo.GuildRoles.TryGetValue(Context.Guild.Id, out roledict);
            roledict.TryGetValue(role, out rolo);
            await ReplyAsync($"{rolo.ToString()}", false);
        }
    }
}
