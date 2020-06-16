using BorisGangBot_Mk2.Services.GuildInfo;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorisGangBot_Mk2.Modules.RoleModules
{
    [Name("Role Management")]
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
        [Alias("Roleme", "role")]
        [Summary("Grants user the requested role.")]
        public async Task RoleMeAsync(string role)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            SocketRole rolo;
            Dictionary<string, SocketRole> guildRoles;
            string rolelower = role.ToLower();

            IEnumerable<string> userroles = user.Roles.Select(r => r.Name.ToLower());
            var userRolesList = userroles.ToList();

            bool getGuildRolesBool = _guildinfo.GuildRoles.TryGetValue(Context.Guild.Id, out guildRoles);

            if (!getGuildRolesBool)
            {
                await ReplyAsync($"Something went wrong collecting the roles for {Context.Guild.Name}. Please try again in a moment.");
                // Some kinda function to retry guild role collection 
                return;
            }

            if (userRolesList.Contains(rolelower))
            {
                await ReplyAsync($"{Context.User.Mention} You are already assigned the role {role}.");
                return;
            }
            try
            {
                if (!(userRolesList.Contains("BorisGoon")) && !(userRolesList.Contains("BorisGang")))
                {
                    guildRoles.TryGetValue("borisgoon", out rolo);
                    await user.AddRoleAsync(rolo);
                    guildRoles.TryGetValue(rolelower, out rolo);
                    await user.AddRoleAsync(rolo);
                }
                else
                {
                    guildRoles.TryGetValue(rolelower, out rolo);
                    await user.AddRoleAsync(rolo);
                }
            }
            catch (Discord.Net.HttpException e)
            {
                System.Net.HttpStatusCode sc = System.Net.HttpStatusCode.Forbidden;
                if (e.HttpCode == sc)
                    await ReplyAsync($"{Context.User.Mention} You don't have permission to receive that role.");

                return;
            }
            await ReplyAsync($"{Context.User.Mention} Your roles have been updated.");
        }

        [Command("removerole")]
        [Summary("Removes the role that you have specified.")]
        public async Task RemoveRoleAsync(string role)
        {
            SocketGuildUser user = Context.Guild.GetUser(Context.User.Id);
            SocketRole rolo;
            Dictionary<string, SocketRole> guildRoles;
            string rolelower = role.ToLower();

            IEnumerable<string> userroles = user.Roles.Select(r => r.Name.ToLower());

            _guildinfo.GuildRoles.TryGetValue(Context.Guild.Id, out guildRoles);

            if (userroles.Contains(rolelower))
            {
                guildRoles.TryGetValue(rolelower, out rolo);
                await user.RemoveRoleAsync(rolo);
                await ReplyAsync($"{Context.User.Mention} Your roles have been updated.");
            }
            else
            {
                await ReplyAsync($"{Context.User.Mention} You don't have that role. Did you spell it correctly?");
            }
        }
    }
}
