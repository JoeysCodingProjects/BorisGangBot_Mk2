using BorisGangBot_Mk2.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BorisGangBot_Mk2.Services.LiveStreamMono.Events.LiveStreamMono
{
    public class OnStreamUpdateArgs : EventArgs
    {
        public string StreamName;

        public StreamModel Stream;
    }
}
