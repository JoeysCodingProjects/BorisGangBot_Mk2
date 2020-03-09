using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BorisGangBot_Mk2.Models;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TwitchLib;
using TwitchLib.Api;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Helix.Models.Streams;

namespace BorisGangBot_Mk2.Modules
{
    [Name("Live Streams")]
    public class LiveModule : ModuleBase<SocketCommandContext>
    {
        private static TwitchAPI api;
        private readonly IConfigurationRoot _config;
        private GetStreamsResponse response = new GetStreamsResponse();

        public LiveModule(IConfigurationRoot config)
        {
            _config = config;
        }

        [Command("live")]
        [Summary("Lists all currently streaming members of Boris Gang.")]
        public async Task Live()
        {
            List<string> streams = new List<string>();

            api = new TwitchAPI();
            api.Settings.ClientId = _config["tokens:tw_cID"];
            api.Settings.AccessToken = _config["tokens:tw_token"];

            int count = 0;
            while (!string.IsNullOrWhiteSpace(_config[$"streams:s{count}"]))
            {
                streams.Add(_config[$"streams:s{count}"]);
                count++;
            }

            response = await api.Helix.Streams.GetStreamsAsync(null, null, 50, null, null, "all", null, streams);

            await ReplyAsync(streams[0], false);
        }
    }
}
