using BorisGangBot_Mk2.Models;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Helix.Models.Streams;
using TwitchLib.Api.Helix.Models.Users;
using TwitchLib.Api.Interfaces;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using TwitchLib.Api.V5.Models.Streams;
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
            StreamModels = new Dictionary<string, StreamModel>();
            await Task.Run(() => GetStreamerList());

            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: GUILD COUNT {_discord.Guilds.Count}");

            List<SocketTextChannel> notifChannels = new List<SocketTextChannel>();
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
                        notifChannels.Add(IEchannels.Current);
                        break;
                    }
                    IEchannels.MoveNext();
                }
                currentPos = 0;
                IEchannels.Dispose();
                IEguilds.MoveNext();
            }
            IEguilds.Dispose();

            StreamNotifChannels = notifChannels;

            if (StreamNotifChannels.Count != 0)
            {
                Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Successfully collected Stream Update Notification channels.");
            }
            else
            {
                Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService ERROR]: No Stream Update Notification channels were found!");
            }

            StreamProfileImages = await GetProfImgUrlsAsync(StreamList);

            LiveStreamMonitor = new LiveStreamMonitorService(TwAPI, UpdInt, 100);

            //LiveStreamMonitor.OnServiceTick += LiveStreamMonitor_OnServiceTick;
            LiveStreamMonitor.OnChannelsSet += OnChannelsSetEvent;
            LiveStreamMonitor.OnServiceStarted += OnServiceStartedEvent;
            LiveStreamMonitor.OnServiceStopped += OnServiceStoppedEvent;
            LiveStreamMonitor.OnStreamOnline += OnStreamOnlineEventAsync;

            LiveStreamMonitor.SetChannelsByName(StreamList);

            LiveStreamMonitor.Start();

            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Was service enabled? - {LiveStreamMonitor.Enabled}");
        }

        #endregion


        #region Events
        // -----
        // Events
        // -----

        private void LiveStreamMonitor_OnServiceTick(object sender, OnServiceTickArgs e)
        {
            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} SERVICE TICK");
        }

        private void OnServiceStartedEvent(object sender, OnServiceStartedArgs e)
        {
            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Live Stream Monitor Service started successfully.");
        }

        private void OnServiceStoppedEvent(object sender, OnServiceStoppedArgs e)
        {
            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Live Stream Monitor Service has been stopped.");
        }

        private async void OnStreamOnlineEventAsync(object sender, OnStreamOnlineArgs e)
        {
            List<string> gameTemp = new List<string>
            {
                e.Stream.GameId
            };

            GetGamesResponse getGamesResponse = await TwAPI.Helix.Games.GetGamesAsync(gameTemp);

            UpdateLiveStreamModelsAsync(e.Channel.ToLower(), e.Stream, getGamesResponse);
            Console.Out.WriteLine(StreamModels.Keys.Contains(e.Channel) + " " + e.Channel + " " + e.Stream.UserName);
            CreateStreamerEmbed(StreamModels[e.Stream.UserName]);
            foreach (SocketTextChannel x in StreamNotifChannels)
            {
                await x.SendMessageAsync(null, false, StreamEmbeds[e.Stream.UserName].Build());
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

        private void UpdateLiveStreamModelsAsync(string streamToModel, TwitchLib.Api.Helix.Models.Streams.Stream twStream, GetGamesResponse gamesResponse)
        {
            List<string> gameIdTemp = new List<string> { twStream.GameId };

            string gameName = gamesResponse.Games.Length != 0 ? gamesResponse.Games[0].Name : null;

            if (StreamModels.ContainsKey(streamToModel))
            { // Just update Title, Game, and Viewers
                StreamModels[streamToModel].Game = gameName;
                StreamModels[streamToModel].Title = twStream.Title;
                StreamModels[streamToModel].Viewers = twStream.ViewerCount;
                return;
            }

            StreamModel streamModel = new StreamModel()
            {
                Stream = twStream.UserName,
                Avatar = StreamProfileImages[streamToModel],
                Title = twStream.Title,
                Game = gameName,
                Viewers = twStream.ViewerCount,
                Link = $"https://www.twitch.tv/{twStream.UserName}"
            };

            StreamModels.Add(streamModel.Stream, streamModel);
        }

        private async Task<Dictionary<string, string>> GetProfImgUrlsAsync(List<string> streams)
        {
            Dictionary<string, string> profImages = new Dictionary<string, string>();

            GetUsersResponse usersResponse = await TwAPI.Helix.Users.GetUsersAsync(null, streams, TwAPI.Settings.AccessToken);

            foreach (var user in usersResponse.Users)
            {
                profImages.Add(user.Login, user.ProfileImageUrl);
            }

            return profImages;
        }

        public void CreateStreamerEmbed(StreamModel streamModel)
        {
            if (StreamEmbeds.ContainsKey(streamModel.Stream))
            {
                EmbedBuilder embed = StreamEmbeds[streamModel.Stream];

                embed.Title = streamModel.Title;
                embed.Fields[0].Value = streamModel.Game != null ? streamModel.Game : "Unknown";
                embed.Fields[1].Value = streamModel.Viewers;

                StreamEmbeds[streamModel.Stream.ToLower()] = embed;
                return;
            }

            
            var a = new EmbedAuthorBuilder()
            {
                Name = streamModel.Stream,
                IconUrl = streamModel.Avatar
            };
            var eb = new EmbedBuilder()
            {
                Author = a,
                Color = new Color(0, 200, 0),
                ThumbnailUrl = streamModel.Avatar,
                Title = streamModel.Title,
                Url = streamModel.Link
            };
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "**Playing:**";
                if (streamModel.Game == null)
                {
                    x.Value = "Unknown";
                }
                else
                {
                    x.Value = streamModel.Game;
                }
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "**Viewers:**";
                x.Value = streamModel.Viewers;
            });

            StreamEmbeds.Add(streamModel.Stream, eb);
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
