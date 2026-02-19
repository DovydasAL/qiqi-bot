using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QiQiBot.BotCommands;
using QiQiBot.Services;

namespace QiQiBot.HostedServices;

internal sealed class BotService : IHostedService, IAsyncDisposable
{
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _config;
    private readonly ILogger<BotService> _logger;

    public BotService(
        DiscordSocketClient client,
        IServiceProvider serviceProvider,
        IConfiguration config,
        ILogger<BotService> logger)
    {
        _client = client;
        _serviceProvider = serviceProvider;
        _config = config;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.Ready += OnClientReadyAsync;
        _client.SlashCommandExecuted += OnSlashCommandExecutedAsync;
        _client.Log += OnClientLogAsync;

        var token = _config.GetValue<string>("DiscordBotToken");
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogCritical("Discord bot token is not configured.");
            throw new InvalidOperationException("Discord bot token is not configured.");
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // optional: detach handlers if you plan to reuse the client
        _client.Ready -= OnClientReadyAsync;
        _client.SlashCommandExecuted -= OnSlashCommandExecutedAsync;
        _client.Log -= OnClientLogAsync;

        await _client.StopAsync();
    }

    public ValueTask DisposeAsync()
    {
        // if you own the client lifetime, dispose it here
        // _client.Dispose();
        return ValueTask.CompletedTask;
    }

    private async Task OnClientReadyAsync()
    {
        _logger.LogInformation("Bot is ready");

        var applicationCommandProperties = new List<ApplicationCommandProperties>
        {
            ClanRegisterCommand.BuildCommand(),
            ClanActivityCommand.BuildCommand(),
            ClanSetAchievementChannel.BuildCommand(),
            ClanSetCitadelResetCommand.BuildCommand(),
            ClanCappedCommand.BuildCommand(),
            ClanSetLeaveJoinChannel.BuildCommand(),
        };

        try
        {
            await _client.BulkOverwriteGlobalApplicationCommandsAsync(
                applicationCommandProperties.ToArray());

            _logger.LogInformation("Registered global application commands.");
        }
        catch (HttpException exception)
        {
            _logger.LogError(exception, "Failed to register application commands");
            // let the host crash this service so container/orchestrator can restart
            throw;
        }
    }

    private async Task OnSlashCommandExecutedAsync(SocketSlashCommand command)
    {
        using var scope = _serviceProvider.CreateScope();

        var clanService = scope.ServiceProvider.GetRequiredService<IClanService>();
        // clanService is not used here right now, but you might use it later;
        // if not needed, you can remove this resolution.

        IBotCommand? handler = command.CommandName switch
        {
            var name when name == ClanActivityCommand.Name
                => scope.ServiceProvider.GetRequiredService<ClanActivityCommand>(),
            var name when name == ClanRegisterCommand.Name
                => scope.ServiceProvider.GetRequiredService<ClanRegisterCommand>(),
            var name when name == ClanSetAchievementChannel.Name
                => scope.ServiceProvider.GetRequiredService<ClanSetAchievementChannel>(),
            var name when name == ClanSetCitadelResetCommand.Name
                => scope.ServiceProvider.GetRequiredService<ClanSetCitadelResetCommand>(),
            var name when name == ClanCappedCommand.Name
                => scope.ServiceProvider.GetRequiredService<ClanCappedCommand>(),
            var name when name == ClanSetLeaveJoinChannel.Name
                => scope.ServiceProvider.GetRequiredService<ClanSetLeaveJoinChannel>(),
            _ => null
        };

        if (handler is null)
        {
            await command.RespondAsync(
                "Sorry, an error occurred while processing your command.");
            _logger.LogWarning("No handler registered for command {CommandName}.", command.CommandName);
            return;
        }

        try
        {
            await handler.Handle(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while handling command {CommandName}.", command.CommandName);
            if (!command.HasResponded)
            {
                await command.RespondAsync(
                    "Sorry, an unexpected error occurred while processing your command.");
            }
        }
    }

    private Task OnClientLogAsync(LogMessage msg)
    {
        // You might want to map Discord log severity to ILogger levels
        _logger.LogInformation("{Source}: {Message}", msg.Source, msg.Message);
        return Task.CompletedTask;
    }
}
