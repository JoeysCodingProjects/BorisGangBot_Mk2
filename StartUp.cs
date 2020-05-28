using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using BorisGangBot_Mk2.Services;
using BorisGangBot_Mk2.Services.GuildInfo;

namespace BorisGangBot_Mk2
{
    class StartUp
    {
        private IConfigurationRoot Configuration { get; }

        public StartUp(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddYamlFile("_config.yml");
            Configuration = builder.Build();
        }

        public static async Task RunAsync(string[] args)
        {
            var startup = new StartUp(args);
            await startup.RunAsync();

        }

        private async Task RunAsync()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<LoggingService>();
            provider.GetRequiredService<CommandHandler>();
            provider.GetRequiredService<GuildJoinedService>();
            provider.GetRequiredService<StreamMonoService>();
            provider.GetRequiredService<GuildInfoService>();

            await provider.GetRequiredService<StartUpService>().StartAsync();

            await Task.Delay(-1);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 100
            }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton<CommandHandler>()
                .AddSingleton<StartUpService>()
                .AddSingleton<LoggingService>()
                .AddSingleton<GuildJoinedService>()
                .AddSingleton<StreamMonoService>()
                .AddSingleton<GuildInfoService>()
                .AddSingleton<Random>()
                .AddSingleton(Configuration);
        }
    }
}
