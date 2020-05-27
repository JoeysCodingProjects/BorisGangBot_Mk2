using BorisGangBot_Mk2.Helpers;
using Discord.Commands;
using Microsoft.VisualBasic;
using System;
using System.Threading.Tasks;

namespace BorisGangBot_Mk2.Modules.BGMembersModules
{
    [Name("Manage Streamers")]
    [HideModuleName]
    [RequireUserPermission(Discord.GuildPermission.Administrator)]
    public class AddStreamerModule : ModuleBase<SocketCommandContext>
    {
        public AddStreamerModule()
        {
        }

        // AddStreamerAsync
        //
        // Parameters:
        // streamer (string)

        #region AddStreamerAsync
        [Command("addstreamer")]
        [Summary("Manually add a streamer to the list of streamers.")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task AddStreamerAsync(string streamer)
        {
            StreamerFileHelper addStreamer = new StreamerFileHelper();
            bool t = await addStreamer.TryAddStreamerAsync(streamer);

            try
            {
                if (t)
                {
                    await ReplyAsync($"Successfully added {streamer}!", false);
                }
                else
                {
                    await ReplyAsync("That streamer is already on the list.", false);
                }
            }
            catch (Exception e)
            {
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
        [Summary("Removes the mentioned streamer from the list.")]
        [RequireUserPermission(Discord.GuildPermission.Administrator)]
        public async Task RemoveStreamerAsync(string streamer)
        {
            StreamerFileHelper sfh = new StreamerFileHelper();
            bool t = await sfh.TryRemoveStreamerAsync(streamer);

            try
            {
                if (t)
                {
                    await ReplyAsync($"Successfully removed {streamer}.");
                }
                else
                {
                    await ReplyAsync($"That streamer was not found. Double check your spelling or make sure they are on the list using the ;streamers command.");
                }
            }
            catch (Exception e)
            {
                await ReplyAsync("Something went wrong trying to remove this streamer.");
            }
        }
        #endregion
    }
}
