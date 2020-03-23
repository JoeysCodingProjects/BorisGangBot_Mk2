using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace BorisGangBot_Mk2.Modules.BotAdminModules
{
    public class ConfigModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _config;

        public ConfigModule(IConfigurationRoot config)
        {
            _config = config;
        }

        [Command("addstreamer")]
        [Summary("Used by Bot Admins to add streamers to the Boris Gang streamers list.")]
        public async Task AddStreamer(string streamer_name)
        {
            List<string> streams = new List<string>();
            using (var reader = new StringReader(File.ReadAllText("./streams.yml")))
            {
                var deserializer = new Deserializer();

                streams = deserializer.Deserialize<List<string>>(reader);
                reader.Close();
            }

            if (streams.Contains(streamer_name))
            {
                await ReplyAsync($"{streamer_name} is already in my list of streamers!");
            }

            await ReplyAsync("Streamer added successfully", false);
        }
    }
}
