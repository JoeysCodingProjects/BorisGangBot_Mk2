using BorisGangBot_Mk2.Models;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using TwitchLib.Api;
using TwitchLib.Api.Interfaces;

namespace BorisGangBot_Mk2.Services.LiveStreamMono
{
    public abstract class StreamMonoServiceBase
    {
        private int _updint;
        private List<string> _streamlist;

        // Twitch API
        public ITwitchAPI TwAPI;

        // Timer Update Interval in milliseconds
        public int UpdInt
        {
            get { return _updint; }
            set { _updint = value; }
        }

        // List of Streamers by UserLogin
        public List<string> StreamList
        {
            get { return _streamlist; }
            set { _streamlist = value; }
        }

        public Dictionary<string, StreamModel> StreamModels { get; set; }

        public Dictionary<string, string> StreamProfileImages { get; set; }

        public Dictionary<string, EmbedBuilder> StreamEmbeds = new Dictionary<string, EmbedBuilder>();

        public List<SocketTextChannel> StreamNotifChannels { get; set; }

        public string NotifChannelName { get; protected set; }
    }
}
