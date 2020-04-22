using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;

namespace BorisGangBot_Mk2.Helpers
{
    public class AddStreamerHelper
    {
        private IConfigurationRoot _config;

        public AddStreamerHelper(IConfigurationRoot config)
        {
            _config = config;
        }

        public bool AddStreamer(string streamer)
        {
            List<string> streamslist = new List<string>();

            Deserializer deserializer = new Deserializer();
            Serializer serializer = new Serializer();
            string result;

            using (StreamReader reader = File.OpenText("./Streamers.yml"))
            {
                result = reader.ReadToEnd();
                reader.Close();
            }

            streamslist = deserializer.Deserialize<List<string>>(result);
            
            if (streamslist.Contains(streamer.ToLower()))
            {
                return false;
            }

            streamslist.Add(streamer.ToLower());
            object newlist = serializer.Serialize(streamslist);

            File.WriteAllText("./Streamers.yml", newlist.ToString());

            return true;
        }
    }
}
