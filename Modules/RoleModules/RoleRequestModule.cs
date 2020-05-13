using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BorisGangBot_Mk2.Services.GuildInfo;
using Discord.Commands;
using Discord.WebSocket;

namespace BorisGangBot_Mk2.Modules.RoleModules
{
    [Name("Role Request Module")]
    public class RoleRequestModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordSocketClient _discord;
        private readonly GuildInfoService _guildinfo;

        public RoleRequestModule(DiscordSocketClient discord, GuildInfoService guildinfo)
        {
            _discord = discord;
            _guildinfo = guildinfo;
        }

        [Command("roleme")]
        [Summary("Grants user the requested role.")]
        public async Task RoleMeAsync(string role)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            SocketRole rolo;
            Dictionary<string, SocketRole> guildRoles;
            string userroles = user.Roles.GetEnumerator().ToString().ToLower();

            _guildinfo.GuildRoles.TryGetValue(Context.Guild.Id, out guildRoles);

            if (userroles.Contains(role))
            {
                await ReplyAsync($"You are already assigned the role {role}.");
            }
            else if (!(_guildinfo.RoleMeEnabledRoles.Contains(role.ToLower())))
            {
                await ReplyAsync("Invalid role. Either it doesn't exist or you aren't allowed to self assign it.");
            }
            else if (!(user.Roles.ToString().Contains("BorisGoon")) && !(user.Roles.ToString().Contains("BorisGang")))
            {
                guildRoles.TryGetValue("borisgoon", out rolo);
                await user.AddRoleAsync(rolo);
                guildRoles.TryGetValue(role.ToLower(), out rolo);
                await user.AddRoleAsync(rolo);
            }
            else
            { 
                guildRoles.TryGetValue(role.ToLower(), out rolo);
                await user.AddRoleAsync(rolo);
            }
            
        }
    }
}
