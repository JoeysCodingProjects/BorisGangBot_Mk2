using BorisGangBot_Mk2.Models;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Games;
using TwitchLib.Api.Helix.Models.Users;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;
using YamlDotNet.Serialization;

namespace BorisGangBot_Mk2.Services.LiveStreamMono
{
    public class StreamMonoService : LiveStreamMono.StreamMonoServiceBase
    {
        private readonly IConfigurationRoot _config;
        private readonly DiscordSocketClient _discord;
        private LiveStreamMonitorService _liveStreamMonitor;
        private LoggingService _logging;



        public StreamMonoService(IConfigurationRoot config,
            DiscordSocketClient discord,
            LoggingService logging)
        {
            _config = config;
            _discord = discord;
            _logging = logging;
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
        public async Task CreateStreamMonoAsync()
        {
            StreamModels = new Dictionary<string, StreamModel>();
            await Task.Run(GetStreamerList);
            await GetStreamerIdDictAsync();

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
                await Console.Out.WriteLineAsync($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Successfully collected Stream Update Notification channels.");
            }
            else
            {
                await Console.Out.WriteLineAsync($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService ERROR]: No Stream Update Notification channels were found!");
            }

            try
            {
                StreamProfileImages = await GetProfImgUrlsAsync(StreamIdList);
            }
            catch (TwitchLib.Api.Core.Exceptions.InternalServerErrorException ex)
            {
                if (CreationAttempts == 5)
                {
                    await Console.Out.WriteLineAsync($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Maximum number of creation attempts exceeded. Live Stream Monitor Service is no longer available.");
                    return;
                }
                await Console.Out.WriteLineAsync($"{ex.GetType().Name} - Attempt #{CreationAttempts}: Error collecting Profile Images. Verify the streamers and then start the service again.");
                VerifyAndGetStreamIdAsync().RunSynchronously();
                CreationAttempts++;
                await CreateStreamMonoAsync();
            }

            _liveStreamMonitor = new LiveStreamMonitorService(TwApi, UpdInt, 100);

            _liveStreamMonitor.OnServiceTick += OnServiceTickEvent;
            _liveStreamMonitor.OnChannelsSet += OnChannelsSetEvent;
            _liveStreamMonitor.OnServiceStarted += OnServiceStartedEvent;
            _liveStreamMonitor.OnServiceStopped += OnServiceStoppedEvent;
            _liveStreamMonitor.OnStreamOnline += OnStreamOnlineEventAsync;

            _liveStreamMonitor.SetChannelsById(StreamIdList);

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

            GetGamesResponse getGamesResponse = new GetGamesResponse();
            try
            {
                getGamesResponse = await TwApi.Helix.Games.GetGamesAsync(gameTemp);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"{ex.GetType().Name}: Error at GetGamesResponse - {ex.Message}");
            }

            try
            {
                UpdateLiveStreamModelsAsync(e.Stream, getGamesResponse);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"{ex.GetType().Name}: Error at UpdateLiveStreamModelsAsync - {ex.Message}");
            }

            EmbedBuilder eb = CreateStreamerEmbed(StreamModels[e.Stream.UserId]);

            foreach (var x in StreamNotifChannels)
            {
                await x.SendMessageAsync(null, false, eb.Build());
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

        #endregion

        #region Monitor Service Required Functions
        // -----
        // Monitor Service Required Functions
        // -----

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

        private async Task GetStreamerIdDictAsync()
        {
            Deserializer deserializer = new Deserializer();
            string result;

            using (StreamReader reader = File.OpenText("./Streamids.yml"))
            {
                result = await reader.ReadToEndAsync();
                reader.Close();
            }

            StreamIds = deserializer.Deserialize<Dictionary<string, string>>(result);
            StreamIdList = StreamIds.Values.AsEnumerable().ToList(); 
            await Console.Out.WriteLineAsync($"{DateTime.UtcNow.ToString("hh:mm:ss")} [StreamMonoService]: Streamer ID List Creation finished.");
        }

        private void UpdateLiveStreamModelsAsync(TwitchLib.Api.Helix.Models.Streams.Stream twitchStream,
            GetGamesResponse game)
        {
            string gameName = game.Games.Length != 0 ? game.Games[0].Name : "Unknown";

            StreamModel streamModel = new StreamModel()
            {
                Stream = twitchStream.UserName,
                Id = twitchStream.UserId,
                Avatar = StreamProfileImages[twitchStream.UserId],
                Title = twitchStream.Title,
                Game = gameName,
                Viewers = twitchStream.ViewerCount,
                Link = $"https://www.twitch.tv/{twitchStream.UserName}"
            };

            if (StreamModels.ContainsKey(twitchStream.UserId))
                StreamModels.Remove(twitchStream.UserId);

            StreamModels.Add(twitchStream.UserId, streamModel);
        }

        private async Task<Dictionary<string, string>> GetProfImgUrlsAsync(List<string> streamIds)
        {
            Dictionary<string, string> profImages = new Dictionary<string, string>();

            GetUsersResponse usersResponse = await TwApi.Helix.Users.GetUsersAsync(streamIds, null, TwApi.Settings.AccessToken);

            foreach (var user in usersResponse.Users)
            {
                profImages.Add(user.Id, user.ProfileImageUrl);
            }

            return profImages;
        }

        private EmbedBuilder CreateStreamerEmbed(StreamModel streamModel)
        {
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
                x.Value = streamModel.Game;
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "**Viewers:**";
                x.Value = streamModel.Viewers;
            });

            return eb;
        }

        public async Task VerifyAndGetStreamIdAsync()
        {
            Serializer serializer = new Serializer();
            Dictionary<string, string> streamsidsDict = new Dictionary<string, string>();
            List<string> verifiedStreams = new List<string>();
            List<string> tmp = new List<string>()
                { " " };

            foreach (string s in StreamList)
            {
                tmp[0] = s;
                await Console.Out.WriteLineAsync($"Current streamer name: {s}");
                try
                {
                    GetUsersResponse response = await TwApi.Helix.Users.GetUsersAsync(logins: tmp, accessToken: TwApi.Settings.AccessToken);
                    streamsidsDict.Add(response.Users[0].Login, response.Users[0].Id);
                    verifiedStreams.Add(s);
                    Thread.Sleep(5000); // So I don't shit on the api too hard :(
                }
                catch (TwitchLib.Api.Core.Exceptions.InternalServerErrorException ex)
                {
                    await Console.Out.WriteLineAsync($"{ex.GetType().Name}: GetUser failed at {s}. Deleting...");
                }
            }

            object streamsFinal = serializer.Serialize(verifiedStreams);
            object dictFinal = serializer.Serialize(streamsidsDict);
            await File.WriteAllTextAsync("./Streamids.yml", dictFinal.ToString());
            await File.WriteAllTextAsync("./Streamers.yml", streamsFinal.ToString());

            await UpdateChannelsToMonitor();
        }

        #endregion

        #region General Purpose Functions
        // -----
        // General Purpose Functions
        // -----

        public async Task UpdateChannelsToMonitor()
        {
            await GetStreamerIdDictAsync();
            _liveStreamMonitor.SetChannelsById(StreamIdList);
            GetStreamerList();
        }

        public bool StopLsm()
        {
            if (!_liveStreamMonitor.Enabled)
                return false;

            _liveStreamMonitor.Stop();
            return true;
        }

        public bool StartLsm()
        {
            if (_liveStreamMonitor.Enabled)
                return false;

            _liveStreamMonitor.Start();
            return true;
        }

        public string StatusLsm()
        {
            return _liveStreamMonitor.Enabled ? "Online" : "Offline";
        }

        #endregion
    }
}
