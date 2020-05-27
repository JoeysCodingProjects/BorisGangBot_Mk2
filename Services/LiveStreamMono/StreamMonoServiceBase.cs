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

        public List<StreamModel> StreamModels { get; set; }

        public List<EmbedBuilder> StreamEmbeds { get; set; }

        public List<SocketTextChannel> StreamNotifChannels = new List<SocketTextChannel>();

        public string NotifChannelName { get; protected set; }
    }
}
