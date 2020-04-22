using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BorisGangBot_Mk2.Models;
using Discord;
using Microsoft.Extensions.Configuration;
using TwitchLib;
using TwitchLib.Api;
using TwitchLib.Api.Helix;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Helix.Models.Streams;
using TwitchLib.Api.Helix.Models.Users;

namespace BorisGangBot_Mk2.Helpers
{
    public class StreamHelpers
    {
        // Returns a list of StreamModels for a given list of stream names
        // Params: TwitchAPI, List<string>
        // Returns: List<StreamModel>
        public async Task<List<StreamModel>> BG_GetLiveStreams(TwitchAPI api, List<string> s)
        {

            // Key = GameID, Value = Game Name
            Dictionary<string, string> g_dictionary = new Dictionary<string, string>();

            GetGamesResponse g_response = new GetGamesResponse();
            GetStreamsResponse s_response = new GetStreamsResponse();
            GetUsersResponse u_response = new GetUsersResponse();

            List<string> g_IDs = new List<string>();
            List<string> s_live = new List<string>();
            List<StreamModel> streamModels = new List<StreamModel>();

            s_response = await api.Helix.Streams.GetStreamsAsync(null, null, s.Count, null, null, "all", null, s);
            foreach (Stream x in s_response.Streams)
            {
                // Only get the avatars of streams that are live
                // Not doing this results in getting all streamer
                // avatars and assigning them to the wrong streamer
                s_live.Add(x.UserName);
            }

            if (s_response.Streams.Length == 0) { return streamModels; }

            u_response = await api.Helix.Users.GetUsersAsync(null, s_live, "hb5w0knsvqeefe73hhho6kbq7tu9x4");

            for (int i = 0; i < s_response.Streams.Length; i++)
            {
                StreamModel stream_daddy = new StreamModel()
                {
                    Stream = s_response.Streams[i].UserName,
                    Avatar = u_response.Users[i].ProfileImageUrl,
                    Title = s_response.Streams[i].Title,
                    Game = s_response.Streams[i].GameId,
                    Viewers = s_response.Streams[i].ViewerCount,
                    Link = $"https://www.twitch.tv/{s_response.Streams[i].UserName}"
                };
                streamModels.Add(stream_daddy);

                if (!g_IDs.Contains(stream_daddy.Game)) { g_IDs.Add(stream_daddy.Game); }
            }

            g_response = await api.Helix.Games.GetGamesAsync(g_IDs, null);
            
            foreach (TwitchLib.Api.Helix.Models.Games.Game x in g_response.Games)
            {
                g_dictionary.TryAdd(x.Id, x.Name);
            }

            string temp;
            foreach (StreamModel x in streamModels)
            {
                g_dictionary.TryGetValue(x.Game, out temp);
                x.Game = temp;
            }

            return streamModels;
        }

        // Creates the EmbedBuilder(s) for all given streams
        // Params: List<StreamModel>
        // Returns: List<EmbedBuilder>
        public List<EmbedBuilder> BG_CreateStreamerEmbeds(List<StreamModel> s)
        {
            List<EmbedBuilder> eb_list = new List<EmbedBuilder>();

            foreach (StreamModel sm in s)
            {
                var a = new EmbedAuthorBuilder()
                {
                    Name = sm.Stream,
                    IconUrl = sm.Avatar
                };
                var eb = new EmbedBuilder()
                {
                    Author = a,
                    Color = new Color(0, 200, 0),
                    ThumbnailUrl = sm.Avatar,
                    Title = sm.Title,
                    Url = sm.Link
                };
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "**Playing:**";
                    x.Value = sm.Game;
                });
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "**Viewers:**";
                    x.Value = sm.Viewers;
                });

                eb_list.Add(eb);
            }

            return eb_list;
        }
    }
}
