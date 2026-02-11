using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QiQiBot.BotCommands;
using QiQiBot.Services;

namespace QiQiBot.HostedServices
{
    internal class BotService : IHostedService, IAsyncDisposable
    {
        private DiscordSocketClient _client;
        private IServiceProvider _serviceProvider;
        private IConfiguration _config;
        private ILogger<BotService> _logger;
        public BotService(DiscordSocketClient client, IServiceProvider serviceProvider, IConfiguration config, ILogger<BotService> logger)
        {
            _client = client;
            _serviceProvider = serviceProvider;
            _config = config;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _client.Ready += async () =>
            {
                _logger.LogInformation("Bot is ready");
                List<ApplicationCommandProperties> applicationCommandProperties = new()
                {
                    ClanRegisterCommand.BuildCommand(),
                    ClanActivityCommand.BuildCommand()
                };

                try
                {
                    await _client.BulkOverwriteGlobalApplicationCommandsAsync(applicationCommandProperties.ToArray());
                }
                catch (HttpException exception)
                {
                    _logger.LogError(exception, "Failed to register application commands");
                    Environment.Exit(1);
                }
            };
            _client.SlashCommandExecuted += async (command) =>
            {
                using var scope = _serviceProvider.CreateScope();
                var clanService = scope.ServiceProvider.GetRequiredService<IClanService>();
                IBotCommand? handler = null;
                if (command.CommandName == ClanActivityCommand.Name)
                {
                    handler = scope.ServiceProvider.GetRequiredService<ClanActivityCommand>();
                }
                else if (command.CommandName == ClanRegisterCommand.Name)
                {
                    handler = scope.ServiceProvider.GetRequiredService<ClanRegisterCommand>();
                }

                if (handler == null)
                {
                    await command.RespondAsync("Sorry, an error occurred while processing your command.");
                    return;
                }
                await handler.Handle(command);
            };
            _client.Log += async (msg) => _logger.LogInformation(msg.ToString());

            var token = _config.GetValue<string>("DiscordBotToken");
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _client.StopAsync();
        }

        public async ValueTask DisposeAsync()
        {
        }

    }
}
