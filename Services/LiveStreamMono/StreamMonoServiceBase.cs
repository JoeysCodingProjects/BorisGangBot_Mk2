using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Api;
using Discord.WebSocket;

namespace BorisGangBot_Mk2.Services.LiveStreamMono
{
    public abstract class StreamMonoServiceBase
    {
        private int _updint;
        private List<string> _streamlist;

        // Twitch API
        public TwitchAPI TwAPI { get; protected set; }

        // Timer Update Interval in milliseconds
        public int UpdInt
        {
            get { return _updint; }
            set { _updint = value * 1000; }
        }

        // List of Streamers by UserLogin
        public List<string> StreamList
        {
            get { return _streamlist; }
            set { _streamlist = value; }
        }
    }
}
