using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BorisGangBot_Mk2.Modules.BotAdminModules
{
    [RequireOwner]
    public class WelcomeInitiationModule : ModuleBase<SocketCommandContext>
    {
        [Command("welcome_initiates")]
        [Summary("Creates the welcome initiates post.")]
        public async Task Welcome_Initiates()
        {
            await ReplyAsync("Temp message explaining which emotes to react with for certain roles.", false);
        }
    }
}
