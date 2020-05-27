using BorisGangBot_Mk2.Models;
using Discord;
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
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using YamlDotNet.Serialization;

namespace BorisGangBot_Mk2.Services
{
    public class StreamMonoService : LiveStreamMono.StreamMonoServiceBase
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _discord;
        public LiveStreamMonitorService LiveStreamMonitor;



        public StreamMonoService(IConfigurationRoot config, DiscordSocketClient discord)
        {
            _config = config;
            _discord = discord;
            _discord.Ready += CreateStreamMonoAsync;


            // Service update interval in seconds
            UpdInt = 30;

            // Name of channel to use when sending notifications
            NotifChannelName = "stream-updates";

            // Assign twitch api credentials
            TwitchAPI _api = new TwitchAPI();
            _api.Settings.ClientId = _config["tokens:tw_cID"];
            _api.Settings.AccessToken = _config["tokens:tw_token"];
            TwAPI = _api;
        }

        #region CreateStreamMonoAsync
        private async Task CreateStreamMonoAsync()
        {
            await Task.Run(() => GetStreamerList());

            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: GUILD COUNT {_discord.Guilds.Count}");


            IEnumerator<SocketGuild> IEguilds = _discord.Guilds.GetEnumerator();
            IEguilds.MoveNext();
            while (IEguilds.Current != null)
            {
                int currentPos = 0;
                Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Current Guild: {IEguilds.Current.Name}");
                IEnumerator<SocketTextChannel> IEchannels = IEguilds.Current.TextChannels.GetEnumerator();
                IEchannels.MoveNext();
                while (currentPos != IEguilds.Current.TextChannels.Count - 1)
                {
                    currentPos++;
                    if (IEchannels.Current.Name.Contains(NotifChannelName))
                    {
                        StreamNotifChannels.Add(IEchannels.Current);
                        break;
                    }
                    IEchannels.MoveNext();
                }
                currentPos = 0;
                IEchannels.Dispose();
                IEguilds.MoveNext();
            }
            IEguilds.Dispose();

            if (StreamNotifChannels.Count != 0)
            {
                Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Successfully collected Stream Update Notification channels.");
            }
            else
            {
                Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService ERROR]: No Stream Update Notification channels were found!");
            }

            LiveStreamMonitor = new LiveStreamMonitorService(TwAPI, UpdInt, 100);

            LiveStreamMonitor.OnServiceTick += LiveStreamMonitor_OnServiceTick;
            LiveStreamMonitor.OnChannelsSet += OnChannelsSetEvent;
            LiveStreamMonitor.OnServiceStarted += OnServiceStartedEvent;
            LiveStreamMonitor.OnStreamOnline += OnStreamOnlineEventAsync;

            LiveStreamMonitor.SetChannelsByName(StreamList);

            LiveStreamMonitor.Start();

            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Was service enabled? - {LiveStreamMonitor.Enabled}");
        }

        private void LiveStreamMonitor_OnServiceTick(object sender, OnServiceTickArgs e)
        {
            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} SERVICE TICK");
        }

        #endregion


        #region Events
        // -----
        // Events
        // -----

        private void OnServiceStartedEvent(object sender, OnServiceStartedArgs e)
        {
            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Live Stream Monitor Service started successfully.");
            LiveStreamMonitor.UpdateLiveStreamersAsync();
        }

        private async void OnStreamOnlineEventAsync(object sender, OnStreamOnlineArgs e)
        {
            List<string> dumbylist = new List<string>();
            dumbylist.Add(e.Channel);
            List<StreamModel> streammodellist = await UpdateLiveStreamModelsAsync(dumbylist);
            List<EmbedBuilder> eblist = BG_CreateStreamerEmbeds(streammodellist);
            foreach (SocketTextChannel x in StreamNotifChannels)
            {
                foreach (EmbedBuilder z in eblist)
                {
                    await x.SendMessageAsync(null, false, z.Build());
                }
            }
        }

        private void OnChannelsSetEvent(object sender, OnChannelsSetArgs e)
        {
            if (!(LiveStreamMonitor.ChannelsToMonitor == null))
            {
                Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Channels to monitor set.");
                return;
            }
            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService ERROR]: Channels to monitor were not properly set!");
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

        #endregion

        #region Monitor Service Required Functions
        // -----
        // Monitor Service Required Functions
        // -----

        public async Task<List<StreamModel>> UpdateLiveStreamModelsAsync(List<string> streamToModel)
        {

            // Key = GameID, Value = Game Name
            Dictionary<string, string> g_dictionary = new Dictionary<string, string>();

            GetGamesResponse g_response = new GetGamesResponse();
            GetStreamsResponse s_response = new GetStreamsResponse();
            GetUsersResponse u_response = new GetUsersResponse();

            List<string> g_IDs = new List<string>();
            List<string> s_live = new List<string>();
            List<StreamModel> streamModels = new List<StreamModel>();

            s_response = await TwAPI.Helix.Streams.GetStreamsAsync(null, null, streamToModel.Count, null, null, "all", null, streamToModel);
            foreach (TwitchLib.Api.Helix.Models.Streams.Stream x in s_response.Streams)
            {
                // Only get the avatars of streams that are live
                // Not doing this results in getting all streamer
                // avatars and assigning them to the wrong streamer
                s_live.Add(x.UserName);
            }

            if (s_response.Streams.Length == 0)
            {
                return streamModels;
            }

            u_response = await TwAPI.Helix.Users.GetUsersAsync(null, s_live, _config["tokens:tw_token"]);

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

            g_response = await TwAPI.Helix.Games.GetGamesAsync(g_IDs, null);

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

        public List<EmbedBuilder> BG_CreateStreamerEmbeds(List<StreamModel> streamModels)
        {
            List<EmbedBuilder> eb_list = new List<EmbedBuilder>();

            foreach (StreamModel sm in streamModels)
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
                    if (sm.Game == null)
                    {
                        x.Value = "Unknown";
                    }
                    else
                    {
                        x.Value = sm.Game;
                    }
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

        #endregion

        #region General Purpose Functions
        // -----
        // General Purpose Functions
        // -----
        //public async Task<bool> TryUpdateStreamFileAsync(string streamer)
        //{
        //    bool fileupdated = false;

        //    return fileupdated;
        //}

        #endregion
    }
}
