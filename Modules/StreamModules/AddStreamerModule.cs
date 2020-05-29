using BorisGangBot_Mk2.Helpers;
using BorisGangBot_Mk2.Services.LiveStreamMono;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using BorisGangBot_Mk2.Services;
using Microsoft.Extensions.Configuration;

namespace BorisGangBot_Mk2.Modules.StreamModules
{
    [Name("Manage Streamers")]
    [RequireUserPermission(Discord.GuildPermission.ManageRoles)]
    public class AddStreamerModule : ModuleBase<SocketCommandContext>
    {
        private readonly StreamMonoService _lsms;
        public AddStreamerModule(StreamMonoService lsms)
        {
            _lsms = lsms;
        }

        // AddStreamerAsync
        //
        // Parameters:
        // streamer (string)

        #region AddStreamerAsync
        [Command("addstreamer")]
        [Summary("Manually add a streamer to the list of streamers. Must have the \"Manage Roles\" permission.")]
        public async Task AddStreamerAsync(string streamer)
        {
            StreamerFileHelper sfh = new StreamerFileHelper(_lsms);
            int t = await sfh.TryAddStreamerAsync(streamer);

            try
            {
                if (t == 1)
                {
                    await ReplyAsync($"Successfully added {streamer}!", false);
                    _lsms.UpdateChannelsToMonitor();
                }
                else if (t == 0)
                {
                    await ReplyAsync("That streamer is already on the list.", false);
                }
                else if (t == -1)
                {
                    await ReplyAsync($"Failed to verify that anyone by the name {streamer} exists on Twitch. Did you spell their name correctly?");
                }
                else
                {
                    await ReplyAsync("I don't know how this happened, but something returned a value that should not be possible.");
                }
            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync(e.Message);
                await ReplyAsync("Something went wrong adding this streamer.");
            }
        }
        #endregion

        // RemoveStreamerAsync
        //
        // Parameters:
        // streamer (string)

        #region RemoveStreamerAsync
        [Command("removestreamer")]
        [Summary("Removes the mentioned streamer from the list. Must have the \"Manage Roles\" permission.")]
        public async Task RemoveStreamerAsync(string streamer)
        {
            StreamerFileHelper sfh = new StreamerFileHelper(_lsms);
            bool t = await sfh.TryRemoveStreamerAsync(streamer);

            try
            {
                if (t)
                {
                    await ReplyAsync($"Successfully removed {streamer}.");
                    _lsms.UpdateChannelsToMonitor();
                }
                else
                {
                    await ReplyAsync($"That streamer was not found. Double check your spelling or make sure they are on the list using the ;streamers command.");
                }
            }
            catch (Exception e)
            {
                await Console.Out.WriteLineAsync(e.Message);
                await ReplyAsync("Something went wrong trying to remove this streamer.");
            }
        }
        #endregion
    }
}
