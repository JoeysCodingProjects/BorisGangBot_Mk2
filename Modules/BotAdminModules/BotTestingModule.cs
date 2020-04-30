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

namespace BorisGangBot_Mk2.Modules.BotAdminModules
{
    [RequireOwner]
    public class BotTestingModule:ModuleBase<SocketCommandContext>
    {
        private IConfigurationRoot _config;
        private StreamMonoService _streams;

        public BotTestingModule(IConfigurationRoot config,
            StreamMonoService streams)
        {
            _config = config;
            _streams = streams;
        }

        [Command("randominfo")]
        [Summary("I still use print statements to test my code because im a naughty boy")]
        public async Task SendRandomInfoAsync()
        {
            await ReplyAsync($"{_streams.StreamList[2]} this many milliseconds", false);
        }
    }
}
