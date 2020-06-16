using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using BorisGangBot_Mk2.Services.LiveStreamMono;
using Microsoft.Extensions.Configuration;
using TwitchLib.Api;

namespace BorisGangBot_Mk2.Helpers
{
    public class StreamerFileHelper
    {
        private readonly StreamMonoService _lsms;
        public StreamerFileHelper(StreamMonoService lsms)
        {
            _lsms = lsms;
        }

        // TryAddStreamerAsync
        //
        // Parameters: 
        // Streamer Name (string)
        //
        // Returns:
        // 

        #region TryAddStreamerAsync(string)
        public async Task<int> TryAddStreamerAsync(string streamer)
        {
            string s = streamer.ToLower();
            string streamerId;

            if (_lsms.StreamList.Contains(s))
                return 0;

            VerifyStreamerHelper verifyStreamer = new VerifyStreamerHelper(_lsms);

            try
            {
                streamerId = await verifyStreamer.TryVerifyStreamerAsync(streamer);
            }
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }

            Deserializer deserializer = new Deserializer();
            Serializer serializer = new Serializer();
            string result;
            string result2;


            using (StreamReader reader = File.OpenText("./Streamers.yml"))
            {
                result = await reader.ReadToEndAsync();
                reader.Close();
            }

            using (StreamReader reader = File.OpenText("./Streamids.yml"))
            {
                result2 = await reader.ReadToEndAsync();
                reader.Close();
            }

            List<string> streamslist = deserializer.Deserialize<List<string>>(result);
            Dictionary<string, string> streamsIdDict = deserializer.Deserialize<Dictionary<string, string>>(result2);

            streamsIdDict.Add(s, streamerId);
            streamslist.Add(s);

            object dictfinal = serializer.Serialize(streamsIdDict);
            object listfinal = serializer.Serialize(streamslist);
            await File.WriteAllTextAsync("./Streamids.yml", dictfinal.ToString());
            await File.WriteAllTextAsync("./Streamers.yml", listfinal.ToString());

            return 1;
        }
        #endregion

        // TryRemoveStreamerAsync
        //
        // Parameters:
        // Streamer Name (string)
        //
        // Returns:
        // true - Streamer successfully removed
        // false - Streamer was not found

        #region TryRemoveStreamerAsync
        public async Task<bool> TryRemoveStreamerAsync(string streamer)
        {
            Deserializer deserializer = new Deserializer();
            Serializer serializer = new Serializer();
            string result;
            string result2;
            string s = streamer.ToLower();

            if (!(_lsms.StreamList.Contains(s)))
                return false;

            using (StreamReader reader = File.OpenText("./Streamers.yml"))
            {
                result = await reader.ReadToEndAsync();
                reader.Close();
            }

            using (StreamReader reader = File.OpenText("./Streamids.yml"))
            {
                result2 = await reader.ReadToEndAsync();
                reader.Close();
            }

            List<string> streamslist = deserializer.Deserialize<List<string>>(result);
            Dictionary<string, string> streamsIdDict = deserializer.Deserialize<Dictionary<string, string>>(result2);

            streamsIdDict.Remove(s);
            streamslist.Remove(s);

            object dictfinal = serializer.Serialize(streamsIdDict);
            object listfinal = serializer.Serialize(streamslist);
            await File.WriteAllTextAsync("./Streamids.yml", dictfinal.ToString());
            await File.WriteAllTextAsync("./Streamers.yml", listfinal.ToString());

            return true;
        }
        #endregion
    }
}
