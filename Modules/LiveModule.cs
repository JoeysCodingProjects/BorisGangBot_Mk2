using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using BorisGangBot_Mk2.Models;
using BorisGangBot_Mk2.Helpers;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using TwitchLib;
using TwitchLib.Api;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Helix.Models.Streams;
using TwitchLib.Api.Helix.Models.Users;
using TwitchLib.Api.Helix.Models.Games;

namespace BorisGangBot_Mk2.Modules
{
    [Name("Live Streams")]
    public class LiveModule : ModuleBase<SocketCommandContext>
    {
        private static TwitchAPI api;
        private readonly IConfigurationRoot _config;

        public LiveModule(IConfigurationRoot config)
        {
            _config = config;
        }

        [Command("live")]
        [Summary("Lists all currently streaming members of Boris Gang.")]
        public async Task Live()
        {
            List<EmbedBuilder> eb_list = new List<EmbedBuilder>();
            List<string> streams = new List<string>();
            List<StreamModel> s_response = new List<StreamModel>();
            StreamHelpers streamHelpers = new StreamHelpers();

            api = new TwitchAPI();
            api.Settings.ClientId = _config["tokens:tw_cID"];
            api.Settings.AccessToken = _config["tokens:tw_token"];

            int count = 0;
            while (!string.IsNullOrWhiteSpace(_config[$"streams:s{count}"]))
            {
                streams.Add(_config[$"streams:s{count}"]);
                count++;
            }

            s_response = await streamHelpers.BG_GetLiveStreams(api, streams);

            if (s_response.Count == 0)
            {
                await ReplyAsync("No members of Boris Gang are currently live.", false);
            }
            else
            {
                eb_list = streamHelpers.BG_CreateStreamerEmbeds(s_response);

                foreach (EmbedBuilder x in eb_list)
                {
                    await ReplyAsync("", false, x.Build());
                }
            }
        }

        [Command("streamers")]
        [Summary("Lists all Boris Gang streams known to the bot.")]
        public async Task Streamers()
        {
            string stream;
            string stream_final = "```\n ";
            int count = 0;

            while (!string.IsNullOrWhiteSpace(_config[$"streams:s{count}"]))
            {
                stream = _config[$"streams:s{count}"];
                stream_final = stream_final.Insert(stream_final.Length - 1, $"{stream} ");
                count++;
            }
            stream_final = stream_final.Insert(stream_final.Length - 1, "\n```");
            await ReplyAsync(stream_final, false);
        }
    }
}
