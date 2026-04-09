using Discord;
using Discord.WebSocket;

namespace QiQiBot.Services;

public sealed class DiscordSocketClientWrapper : IDiscordSocketClientWrapper
{
    private readonly DiscordSocketClient _client;

    public DiscordSocketClientWrapper(DiscordSocketClient client)
    {
        _client = client;
    }

    public event Func<Task>? Ready
    {
        add => _client.Ready += value;
        remove => _client.Ready -= value;
    }

    public event Func<SocketSlashCommand, Task>? SlashCommandExecuted
    {
        add => _client.SlashCommandExecuted += value;
        remove => _client.SlashCommandExecuted -= value;
    }

    public event Func<SocketGuildUser, Task>? UserJoined
    {
        add => _client.UserJoined += value;
        remove => _client.UserJoined -= value;
    }

    public event Func<LogMessage, Task>? Log
    {
        add => _client.Log += value;
        remove => _client.Log -= value;
    }

    public Task LoginAsync(TokenType tokenType, string token)
    {
        return _client.LoginAsync(tokenType, token);
    }

    public Task StartAsync()
    {
        return _client.StartAsync();
    }

    public Task StopAsync()
    {
        return _client.StopAsync();
    }

    public Task BulkOverwriteGlobalApplicationCommandsAsync(ApplicationCommandProperties[] properties)
    {
        return _client.BulkOverwriteGlobalApplicationCommandsAsync(properties);
    }

    public SocketGuild? GetGuild(ulong id)
    {
        return _client.GetGuild(id);
    }
}
