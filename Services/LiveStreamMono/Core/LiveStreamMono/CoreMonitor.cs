using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TwitchLib.Api.Helix.Models.Streams;
using TwitchLib.Api.Interfaces;

namespace BorisGangBot_Mk2.Services.LiveStreamMono.Core.LiveStreamMono
{
    internal abstract class CoreMonitor
    {
        protected readonly ITwitchAPI _api;

        public abstract Task<GetStreamsResponse> GetStreamsAsync(List<string> channels);
        public abstract Task<Func<Stream, bool>> CompareStream(string channel);

        protected CoreMonitor(ITwitchAPI api)
        {
            _api = api;
        }
    }
}
