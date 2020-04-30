using BorisGangBot_Mk2.Models;
using System;

namespace BorisGangBot_Mk2.Services.LiveStreamMono.Events.LiveStreamMono
{
    public class OnStreamOnlineArgs : EventArgs
    {
        public string StreamName;

        public StreamModel Stream;
    }
}
