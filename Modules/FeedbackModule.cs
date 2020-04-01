//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using Discord;
//using Discord.API;
//using Discord.Commands;
//using Discord.WebSocket;
//using Microsoft.Extensions.Configuration;

//namespace BorisGangBot_Mk2.Modules
//{
//    [Name("Feedback Module")]
//    public class FeedbackModule : ModuleBase<SocketCommandContext>
//    {
//        private readonly DiscordSocketClient _discord;
//        private readonly IConfigurationRoot _config;

//        public FeedbackModule(DiscordSocketClient discord, IConfigurationRoot config)
//        {
//            _discord = discord;
//            _config = config;
//        }

//        [Command("feedback")]
//        [Summary("Use this command for things like command suggestions, features, bugs, any streamers with misspelled names or are missing from the ;streamers list, etc.")]
//        public async Task FBStreamer(params string[] list)
//        {
//            IUser dm = _discord.GetUser(insert userid here);
//            await dm.SendMessageAsync($"'{Context.Message.ToString().Replace(";feedback", "")}' - {Context.Message.Author} | {Context.Message.Timestamp.DateTime}");
//            await ReplyAsync("Your message was successfully sent to Joey!", false);
//        }
//    }
//}
