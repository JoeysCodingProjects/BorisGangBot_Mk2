using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace BorisGangBot_Mk2.Modules.BotAdminModules
{
    public class ConfigModule : ModuleBase<SocketCommandContext>
    {
        private readonly IConfigurationRoot _config;
    }
}
