using BorisGangBot_Mk2.Models;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using TwitchLib.Api.Interfaces;

namespace BorisGangBot_Mk2.Services.LiveStreamMono
{
    public abstract class StreamMonoServiceBase
    {
        // Live Stream Mono Creation attempts
        private int _creationattempts = 0;

        protected int CreationAttempts 
        {
            get { return _creationattempts; }
            set
            {
                _creationattempts = value;
            }
        }
        // Twitch API
        public ITwitchAPI TwApi;

        // Timer Update Interval in milliseconds
        protected int UpdInt { get; set; }

        // List of Streamers by UserLogin
        public List<string> StreamList { get; protected set; }

        public List<string> StreamIdList { get; protected set; }

        public Dictionary<string, string> StreamIds { get; protected set; }

        protected Dictionary<string, StreamModel> StreamModels { get; set; }

        protected Dictionary<string, string> StreamProfileImages { get; set; }

        protected List<SocketTextChannel> StreamNotifChannels { get; set; }

        protected string NotifChannelName { get; set; }
    }
}
