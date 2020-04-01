using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Discord;
using Discord.Commands;

namespace BorisGangBot_Mk2.Modules.FunnyModules
{
    public class JosieModule : ModuleBase<SocketCommandContext>
    {
        [Command("josie")]
        [Summary("Replies with a random quote actually spoken by Josie.")]
        public async Task JosieAsync()
        {
            // Storing the quotes in raw code until i set up SQL DB
            // Then I'll add a quotejosie command

            await ReplyAsync("\"Straight men have no rights. Periodt.\" - Josie", false );
        }
    }
}
