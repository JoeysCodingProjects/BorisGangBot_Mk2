using BorisGangBot_Mk2.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using System.IO;

namespace BorisGangBot_Mk2.Modules.BGMembersModules
{
    [RequireUserPermission(Discord.GuildPermission.Administrator)]
    public class AddStreamerModule : ModuleBase<SocketCommandContext>
    {
        private IConfigurationRoot _config;
        

        public AddStreamerModule(IConfigurationRoot config)
        {
            _config = config;
        }

        [Command("addstreamer")]
        [Summary("Manually add a streamer to the list of streamers.")]
        //[RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task AddStreamerAsync(string streamer)
        {
            AddStreamerHelper addStreamer = new AddStreamerHelper(_config);
            bool t = addStreamer.AddStreamer(streamer);

            if (t)
            {
                await ReplyAsync($"Successfully added {streamer}!", false);
            }
            else
            {
                await ReplyAsync("That streamer is already on the list.", false);
            }
        }

        [Command("removestreamer")]
        [Summary("Removes the mentioned streamer from the list.")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task RemoveStreamerAsync(string streamer)
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

            if (streamslist.Contains(streamer))
            {
                streamslist.Remove(streamer);
                object newlist = serializer.Serialize(streamslist);
                File.WriteAllText("./Streamers.yml", newlist.ToString());
                await ReplyAsync($"Streamer {streamer} successfully removed.", false);
            }
            else
            {
                await ReplyAsync($"Could not find {streamer}", false);
            }
        }

    }
}
