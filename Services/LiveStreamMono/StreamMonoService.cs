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
using TwitchLib.Api.Helix.Models.Users;
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
        private LiveStreamMonitorService _liveStreamMonitor;



        public StreamMonoService(IConfigurationRoot config, DiscordSocketClient discord)
        {
            _config = config;
            _discord = discord;
            _discord.Ready += CreateStreamMonoAsync;

            // Assign Config File Variables
            try
            {
                UpdInt = int.Parse(_config["liveStreamMono:updIntervalSeconds"]);
            }
            catch 
            {
                Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Failed to parse Update Interval from _config.yml. Defaulting to 30 seconds.");
                UpdInt = 30;
            }
            NotifChannelName = _config["liveStreamMono:notifChannelName"];

            // Assign twitch api credentials
            TwitchAPI api = new TwitchAPI();
            api.Settings.ClientId = _config["tokens:tw_cID"];
            api.Settings.AccessToken = _config["tokens:tw_token"];
            TwApi = api;
        }

        #region CreateStreamMonoAsync
        private async Task CreateStreamMonoAsync()
        {
            StreamModels = new Dictionary<string, StreamModel>();
            await Task.Run(GetStreamerList);

            await Console.Out.WriteLineAsync($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: GUILD COUNT {_discord.Guilds.Count}");

            List<SocketTextChannel> notifChannels = new List<SocketTextChannel>();
            IEnumerator<SocketGuild> eguilds = _discord.Guilds.GetEnumerator();

            eguilds.MoveNext();
            while (eguilds.Current != null)
            {
                int currentPos = 0;

                await Console.Out.WriteLineAsync($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Current Guild: {eguilds.Current.Name}");
                
                IEnumerator<SocketTextChannel> echannels = eguilds.Current.TextChannels.GetEnumerator();

                echannels.MoveNext();
                while (currentPos != eguilds.Current.TextChannels.Count - 1)
                {
                    currentPos++;
                    if (echannels.Current != null && echannels.Current.Name.Contains(NotifChannelName))
                    {
                        notifChannels.Add(echannels.Current);
                        break;
                    }
                    echannels.MoveNext();
                }
                currentPos = 0;
                echannels.Dispose();
                eguilds.MoveNext();
            }
            eguilds.Dispose();

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

            _liveStreamMonitor = new LiveStreamMonitorService(TwApi, UpdInt, 100);

            _liveStreamMonitor.OnServiceTick += OnServiceTickEvent;
            _liveStreamMonitor.OnChannelsSet += OnChannelsSetEvent;
            _liveStreamMonitor.OnServiceStarted += OnServiceStartedEvent;
            _liveStreamMonitor.OnServiceStopped += OnServiceStoppedEvent;
            _liveStreamMonitor.OnStreamOnline += OnStreamOnlineEventAsync;

            _liveStreamMonitor.SetChannelsByName(StreamList);

            _liveStreamMonitor.Start();

            await Console.Out.WriteLineAsync($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Was service enabled? - {_liveStreamMonitor.Enabled}");
        }

        #endregion


        #region Events
        // -----
        // Events
        // -----

        private void OnServiceTickEvent(object sender, OnServiceTickArgs e)
        {
            
        }

        private static void OnServiceStartedEvent(object sender, OnServiceStartedArgs e)
        {
            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Live Stream Monitor Service started successfully.");
        }

        private static void OnServiceStoppedEvent(object sender, OnServiceStoppedArgs e)
        {
            Console.Out.WriteLine($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Live Stream Monitor Service has been stopped.");
        }

        private async void OnStreamOnlineEventAsync(object sender, OnStreamOnlineArgs e)
        {
            var gameTemp = new List<string>
            {
                e.Stream.GameId
            };

            GetGamesResponse getGamesResponse = await TwApi.Helix.Games.GetGamesAsync(gameTemp);

            UpdateLiveStreamModelsAsync(e.Channel.ToLower(), e.Stream, getGamesResponse);

            await Console.Out.WriteLineAsync(StreamModels.Keys.Contains(e.Channel) + " " + e.Channel + " " + e.Stream.UserName);

            CreateStreamerEmbed(StreamModels[e.Stream.UserName]);
            foreach (var x in StreamNotifChannels)
            {
                await x.SendMessageAsync(null, false, StreamEmbeds[e.Stream.UserName].Build());
            }
        }

        private void OnChannelsSetEvent(object sender, OnChannelsSetArgs e)
        {
            if (_liveStreamMonitor.ChannelsToMonitor != null)
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

            if (StreamModels[streamToModel] != null)
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
            try
            {
                StreamModels.Add(streamModel.Stream, streamModel);
            }
            catch (Exception e)
            {
                Console.Out.WriteLineAsync(e.Message);
            }
        }

        private async Task<Dictionary<string, string>> GetProfImgUrlsAsync(List<string> streams)
        {
            Dictionary<string, string> profImages = new Dictionary<string, string>();

            GetUsersResponse usersResponse = await TwApi.Helix.Users.GetUsersAsync(null, streams, TwApi.Settings.AccessToken);

            foreach (var user in usersResponse.Users)
            {
                profImages.Add(user.Login, user.ProfileImageUrl);
            }

            return profImages;
        }

        private void CreateStreamerEmbed(StreamModel streamModel)
        {
            if (StreamEmbeds.ContainsKey(streamModel.Stream))
            {
                EmbedBuilder embed = StreamEmbeds[streamModel.Stream];

                embed.Title = streamModel.Title;
                embed.Fields[0].Value = streamModel.Game ?? "Unknown";
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
                x.Value = streamModel.Game ?? "Unknown";
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
        public void UpdateChannelsToMonitor()
        {
            GetStreamerList();
            _liveStreamMonitor.SetChannelsByName(StreamList);
        }

        #endregion
    }
}
