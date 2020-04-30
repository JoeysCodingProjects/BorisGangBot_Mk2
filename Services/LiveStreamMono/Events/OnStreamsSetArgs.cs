using System;
using System.Collections.Generic;

namespace BorisGangBot_Mk2.Services.LiveStreamMono.Events
{
    public class OnStreamsSetArgs : EventArgs
    {
        public List<string> Streams;
    }
}
