using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace BorisGangBot_Mk2.Helpers
{
    public class StreamerFileHelper
    {
        public StreamerFileHelper()
        {
        }

        // TryAddStreamerAsync
        //
        // Parameters: 
        // Streamer Name (string)
        //
        // Returns:
        // true - Streamer successfully added
        // false - Streamer already exists

        #region TryAddStreamerAsync(string)
        public async Task<bool> TryAddStreamerAsync(string streamer)
        {
            Deserializer deserializer = new Deserializer();
            Serializer serializer = new Serializer();
            string result;
            string s = streamer.ToLower();


            using (StreamReader reader = File.OpenText("./Streamers.yml"))
            {
                result = await reader.ReadToEndAsync();
                reader.Close();
            }

            List<string> streamslist = deserializer.Deserialize<List<string>>(result);

            if (streamslist.Contains(s))
                return false;


            streamslist.Add(s);
            object listfinal = serializer.Serialize(streamslist);
            await File.WriteAllTextAsync("./Streamers.yml", listfinal.ToString());

            return true;
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
            string s = streamer.ToLower();

            using (StreamReader reader = File.OpenText("./Streamers.yml"))
            {
                result = await reader.ReadToEndAsync();
                reader.Close();
            }

            List<string> streamslist = deserializer.Deserialize<List<string>>(result);

            if (!(streamslist.Contains(s)))
                return false;

            streamslist.Remove(s);
            object listfinal = serializer.Serialize(streamslist);
            await File.WriteAllTextAsync("./Streamers.yml", listfinal.ToString());

            return true;
        }
        #endregion
    }
}
