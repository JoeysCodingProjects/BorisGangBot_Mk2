using BorisGangBot_Mk2.Services;
using Discord.Commands;
using System.Threading.Tasks;
using BorisGangBot_Mk2.Services.LiveStreamMono;

namespace BorisGangBot_Mk2.Modules.StreamModules
{
    [Name("Streamer Related")]
    public class StreamersModule : ModuleBase<SocketCommandContext>
    {
        private readonly StreamMonoService _lsms;

        public StreamersModule(StreamMonoService lsms)
        {
            _lsms = lsms;
        }

        #region Streamers Command
        [Command("streamers")]
        [Alias("streams", "stream")]
        [Summary("Lists all Boris Gang streams known to the bot.")]

        public async Task Streamers()
        {
            string stream_final = "```\n";

            for (int i = 0; i < _lsms.StreamList.Count; i++)
            {
                stream_final += $"{_lsms.StreamList[i]} ";
            }

            stream_final = stream_final.Insert(stream_final.Length - 1, "\n```");
            await ReplyAsync(stream_final, false);
        }
        #endregion
    }
}
