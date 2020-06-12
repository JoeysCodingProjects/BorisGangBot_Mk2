using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BorisGangBot_Mk2.Services;
using BorisGangBot_Mk2.Services.LiveStreamMono;
using Microsoft.Extensions.Configuration;
using TwitchLib.Api.Helix.Models.Streams;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Users;


namespace BorisGangBot_Mk2.Helpers
{
    public class VerifyStreamerHelper
    {
        private readonly StreamMonoService _lsms;

        public VerifyStreamerHelper(StreamMonoService lsms)
        {
            _lsms = lsms;
        }

        public async Task<bool> TryVerifyStreamerAsync(string streamer)
        {
            try
            {
                List<string> tmp = new List<string>();
                tmp.Add(streamer);
                tmp.Add(streamer);

                GetUsersResponse result = await _lsms.TwApi.Helix.Users.GetUsersAsync(logins:tmp, accessToken:_lsms.TwApi.Settings.AccessToken);

                return result.Users[0].Login != null;
            }
            catch (Exception e)
            {
                return false;
            }
        }
}
}
