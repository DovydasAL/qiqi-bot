using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QiQiBot.BotCommands;
using QiQiBot.Exceptions;
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
        _client.UserJoined += OnUserJoinedAsync;
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
        _client.UserJoined -= OnUserJoinedAsync;
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
            ClanSetCitadelChannel.BuildCommand(),
            ClanSetCitadelResetCommand.BuildCommand(),
            ClanCappedCommand.BuildCommand(),
            ClanSetLeaveJoinChannel.BuildCommand(),
            ClanSetWelcomeChannel.BuildCommand(),
            ClanDebugCommand.BuildCommand(),
            ClanRsnAuditCommand.BuildCommand(),
            RsnCommand.BuildCommand(),
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
            var name when name == ClanSetCitadelChannel.Name
                => scope.ServiceProvider.GetRequiredService<ClanSetCitadelChannel>(),
            var name when name == ClanSetCitadelResetCommand.Name
                => scope.ServiceProvider.GetRequiredService<ClanSetCitadelResetCommand>(),
            var name when name == ClanCappedCommand.Name
                => scope.ServiceProvider.GetRequiredService<ClanCappedCommand>(),
            var name when name == ClanSetLeaveJoinChannel.Name
                => scope.ServiceProvider.GetRequiredService<ClanSetLeaveJoinChannel>(),
            var name when name == ClanSetWelcomeChannel.Name
                => scope.ServiceProvider.GetRequiredService<ClanSetWelcomeChannel>(),
            var name when name == ClanDebugCommand.Name
                => scope.ServiceProvider.GetRequiredService<ClanDebugCommand>(),
            var name when name == ClanRsnAuditCommand.Name
                => scope.ServiceProvider.GetRequiredService<ClanRsnAuditCommand>(),
            var name when name == RsnCommand.Name
                => scope.ServiceProvider.GetRequiredService<RsnCommand>(),
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

    private async Task OnUserJoinedAsync(SocketGuildUser user)
    {
        using var scope = _serviceProvider.CreateScope();
        var clanService = scope.ServiceProvider.GetRequiredService<IClanService>();

        try
        {
            var dbGuild = await clanService.GetGuild(user.Guild.Id);
            if (dbGuild.ClanWelcomeChannelId == null)
            {
                _logger.LogTrace("Guild {GuildId} does not have a welcome channel set, skipping welcome message.", user.Guild.Id);
                return;
            }

            var channel = user.Guild.GetTextChannel(dbGuild.ClanWelcomeChannelId.Value);
            if (channel == null)
            {
                _logger.LogWarning("Welcome channel {ChannelId} not found in guild {GuildId}, cannot send welcome message.", dbGuild.ClanWelcomeChannelId.Value, user.Guild.Id);
                return;
            }

            await channel.SendMessageAsync($"Welcome {user.Mention}! Please set your Runescape Username so you can track achievements. You can use the command /rsn followed by a space and then your RuneScape Username.");
        }
        catch (NoClanRegisteredException)
        {
            _logger.LogTrace("Guild {GuildId} is not configured yet, skipping welcome message.", user.Guild.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while sending welcome message for guild {GuildId} and user {UserId}.", user.Guild.Id, user.Id);
        }
    }

    private Task OnClientLogAsync(LogMessage msg)
    {
        // You might want to map Discord log severity to ILogger levels
        _logger.LogInformation("{Source}: {Message}", msg.Source, msg.Message);
        return Task.CompletedTask;
    }
}
