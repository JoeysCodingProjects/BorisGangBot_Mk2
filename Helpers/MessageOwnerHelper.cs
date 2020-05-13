using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace BorisGangBot_Mk2.Helpers
{
    // MessageOwnerHelper
    //
    // Helper function for notifying the Bot Owner via Direct Message
    // of any errors that occur while attempting to execute commands.

    public class MessageOwnerHelper
    {
        private readonly DiscordSocketClient _discord;

        // This is the ID of the Direct Message channel between the bot
        // and it's owner. Used to make sending owner important updates easier.
        private ulong dm_channel_id = 687034508477726748;



        public MessageOwnerHelper(DiscordSocketClient discord)
        {
            _discord = discord;
        }

        // MessageOwnerAsync
        //
        // Parameters:
        // msg (SocketUserMessage)
        // reason (Optional : String)

        #region MessageOwnerAsync
        public async Task MessageOwnerAsync(SocketUserMessage msg, string reason = "Unknown")
        {
            // Create the DMChannel object to send message to owner
            var dmchannel = await _discord.GetDMChannelAsync(dm_channel_id);
            StringBuilder sb = new StringBuilder();

            string cmd = msg.Content.Remove(0, 1);
            if (cmd.Contains(" "))
                cmd = cmd.Split("0")[0];

            sb.Append($"{DateTime.UtcNow.ToString("hh:mm:ss")} [{cmd}]: Failed to execute - ");

            if (reason.Equals("Unknown"))
            {
                sb.Append("No reason specified.");
            }
            else
            {
                sb.Append($"Reason: {reason}");
            }
            
            if (dmchannel == null)
            {
                Console.Out.WriteLine($"{msg.Channel.Id}");
                return;
            }
            await dmchannel.SendMessageAsync("hello");
        }
        #endregion
    }
}
