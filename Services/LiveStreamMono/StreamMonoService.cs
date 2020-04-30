using BorisGangBot_Mk2.Models;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Helix.Models.Streams;
using TwitchLib.Api.Helix.Models.Users;
using YamlDotNet.Serialization;

namespace BorisGangBot_Mk2.Services
{
    public class StreamMonoService : LiveStreamMono.StreamMonoServiceBase
    {
        private IConfigurationRoot _config;
        private readonly DiscordSocketClient _discord;

        public StreamMonoService(IConfigurationRoot config, DiscordSocketClient discord)
        {
            _config = config;
            _discord = discord;
        }

        public async void StartStreamMonoAsync()
        {
            await Task.Run(() => GetStreamerList());
            await Task.Run(() => CreateTwitchAPI());
        }

        private void GetStreamerList()
        {
            Deserializer deserializer = new Deserializer();
            string result;

            using (StreamReader reader = File.OpenText("./Streamers.yml"))
            {
                result = reader.ReadToEnd();
                reader.Close();
            }

            StreamList = deserializer.Deserialize<List<string>>(result);

            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: StreamerList Creation finished.");
        }
        
        private void CreateTwitchAPI()
        {
            TwitchAPI _twapi = new TwitchAPI();
            _twapi.Settings.ClientId = _config["tokens:tw_cID"];
            _twapi.Settings.AccessToken = _config["tokens:tw_token"];

            TwAPI = _twapi;
            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: TwitchAPI Creation finished.");
        }

        // -----
        // Monitor Service Required Functions
        // -----

        public async Task<List<StreamModel>> UpdateLiveStreamsAsync(TwitchAPI api, List<string> s)
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
            foreach (TwitchLib.Api.Helix.Models.Streams.Stream x in s_response.Streams)
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

        // -----
        // General Purpose Functions
        // -----
        public async Task<bool> TryUpdateStreamFileAsync(string streamer)
        {
            bool fileupdated = false;

            return fileupdated;
        }
    }
}
