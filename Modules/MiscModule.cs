using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace BorisGangBot_Mk2.Modules
{
    public class MiscModule : ModuleBase<SocketCommandContext>
    {
        [Name("source")]
        [Summary("Gives a link to Boris Gang Bot's source code.")]
        public async Task SourceAsync()
        {
            await ReplyAsync($"");
        }
    }
}
