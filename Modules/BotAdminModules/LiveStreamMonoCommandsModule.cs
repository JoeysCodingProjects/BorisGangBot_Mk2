using System.Threading.Tasks;
using BorisGangBot_Mk2.Services.LiveStreamMono;
using Discord.Commands;

namespace BorisGangBot_Mk2.Modules.BotAdminModules
{
    [Name("Live Stream Monitor based commands")]
    [RequireUserPermission(Discord.GuildPermission.Administrator)]
    [Group("lsm")]
    public class LiveStreamMonoCommandsModule : ModuleBase<SocketCommandContext>
    {
        private StreamMonoService _streams;

        public LiveStreamMonoCommandsModule(StreamMonoService streams)
        {
            _streams = streams;
        }

        [Command("stop")]
        [Alias("Stop", "STOP")]
        [Summary("Stops the Live Stream Monitor Service.")]
        public async Task LMS_StopAsync()
        {
            bool lsmState = _streams.StopLsm();

            if (lsmState)
            {
                await ReplyAsync("The Live Stream Monitor service has been successfully stopped.");
            }
            else
            {
                await ReplyAsync("Failed to stop the Live Stream Monitor service. Are you sure it's running?");
            }
        }

        [Command("start")]
        [Alias("Start", "START")]
        [Summary("Starts the Live Stream Monitor Service if it is not running.")]
        public async Task LMS_StartAsync()
        {
            bool lsmState = _streams.StartLsm();

            if (lsmState)
            {
                await ReplyAsync("The Live Stream Monitor Service has been successfully started.");
            }
            else
            {
                await ReplyAsync("Failed to start the Live Stream Monitor Service. Are you sure it's not already running?");
            }
        }

        [Command("verifystreams")]
        [Alias("vs", "VerifyStreams", "Verify_Streams", "verify_streams")]
        [Summary("Verifies the list of streamers, removing those who would cause errors or don't exist.")]
        public async Task LMS_VerifyStreamsAsync()
        {
            await ReplyAsync("Verification has begun. This might take a while...");
            await _streams.VerifyAndGetStreamIdAsync();
        }
    }
}
