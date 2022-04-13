using Discord.WebSocket;
using Discord;
using Discord.Addons.Interactive;
using DiscordBot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Discord.Commands;
using DiscordBot.Database;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot
{
    public class Startup
    {
        private readonly IConfigurationRoot _config;
        private readonly IServiceProvider _services;
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;

        public Startup()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile(path: "config.json");
            _config = builder.Build();

            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000
            });

            _commands = new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Verbose,
                CaseSensitiveCommands = false,
                ThrowOnError = false
            });

            _services = ConfigureServices(_client, _commands);
        }

        public async Task StartAsync()
        {
            //run migrations
            await SetUpDatabase();

            await _services.GetService<CommandHandler>().InstallCommandsAsync();
            _client.Log += Log;
            _commands.Log += Log;

            string discordToken = _config["token"];
            if (string.IsNullOrWhiteSpace(discordToken))
            {
                throw new Exception("Token is missing from the config.json file.");
            }

            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static IServiceProvider ConfigureServices(DiscordSocketClient client, CommandService commands)
        {
            var services = new ServiceCollection()
                    .AddSingleton(client)
                    .AddSingleton(commands)
                    .AddSingleton<CommandHandler>()
                    .AddSingleton<InteractiveService>();

            return services.BuildServiceProvider();
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private async Task SetUpDatabase()
        {
            int retryAttempts = 0;
            while (retryAttempts < 10)
            {
                try
                {
                    using (MarketBotContext context = new MarketBotContext())
                    {
                        context.Database.Migrate();
                    }
                    break;
                }
                catch(Exception e)
                {
                    Console.WriteLine("Retrying database connection...");
                    retryAttempts += 1;
                    await Task.Delay(1000);
                }
            }
        }
    }

    
}
