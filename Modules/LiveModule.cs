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
using TwitchLib.Api.Helix.Models.Users;

namespace BorisGangBot_Mk2.Modules
{
    [Name("Live Streams")]
    public class LiveModule : ModuleBase<SocketCommandContext>
    {
        private static TwitchAPI api;
        private readonly IConfigurationRoot _config;
        private GetStreamsResponse response = new GetStreamsResponse();
        private GetUsersResponse u_response = new GetUsersResponse();

        public LiveModule(IConfigurationRoot config)
        {
            _config = config;
        }

        [Command("live")]
        [Summary("Lists all currently streaming members of Boris Gang.")]
        public async Task Live()
        {
            List<string> streams = new List<string>();
            List<StreamModel> streams_response = new List<StreamModel>();

            api = new TwitchAPI();
            api.Settings.ClientId = _config["tokens:tw_cID"];
            api.Settings.AccessToken = _config["tokens:tw_token"];

            int count = 0;
            while (!string.IsNullOrWhiteSpace(_config[$"streams:s{count}"]))
            {
                streams.Add(_config[$"streams:s{count}"]);
                count++;
            }

            response = await api.Helix.Streams.GetStreamsAsync(null, null, 5, null, null, "all", null, streams); // Add streams list back after testing
            streams.Clear();

            count = 0;
            while (count < response.Streams.Length)
            {
                streams.Add(response.Streams[count].UserName);
                count++;
            }

            u_response = await api.Helix.Users.GetUsersAsync(null, streams);

            count = 0;
            while (count < response.Streams.Length)
            {
                StreamModel x = new StreamModel()
                {
                    Stream = response.Streams[count].UserName,
                    Avatar = u_response.Users[count].ProfileImageUrl,
                    Live = true,
                };
                x.Link = $"https://www.twitch.tv/{x.Stream}";

                streams_response.Add(x);
                count++;
            }

            if (streams_response.Count == 0)
            {
                await ReplyAsync("No members of Boris Gang are currently live.", false);
            } else
            {
                foreach (StreamModel s in streams_response)
                {
                    var builder = new EmbedBuilder()
                    {
                        Color = new Color(255, 0, 0),
                        Title = $"{s.Stream} is Live!",
                        ImageUrl = s.Avatar,
                        Url = s.Link
                    };

                    Console.WriteLine(s.Avatar);
                    await ReplyAsync("", false, builder.Build());

                }
            }
        }
    }
}
