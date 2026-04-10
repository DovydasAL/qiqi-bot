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

    public SocketTextChannel? GetTextChannel(ulong guildId, ulong channelId)
    {
        var guild = _client.GetGuild(guildId);
        return guild?.GetTextChannel(channelId);
    }

    public IReadOnlyList<DiscordGuildUserInfo>? GetGuildUsers(ulong guildId)
    {
        var guild = _client.GetGuild(guildId);
        if (guild is null)
        {
            return null;
        }

        return guild.Users
            .Select(u => new DiscordGuildUserInfo(
                u.Id,
                u.Username,
                u.DisplayName,
                u.IsBot))
            .ToList();
    }

    public DiscordGuildUserInfo? GetGuildUser(ulong guildId, ulong userId)
    {
        var guild = _client.GetGuild(guildId);
        var user = guild?.GetUser(userId);
        if (user is null)
        {
            return null;
        }

        return new DiscordGuildUserInfo(user.Id, user.Username, user.DisplayName, user.IsBot);
    }

    public async Task TrySetGuildUserNicknameAsync(ulong guildId, ulong userId, string nickname)
    {
        var guild = _client.GetGuild(guildId);
        var user = guild?.GetUser(userId);
        if (user is null)
        {
            return;
        }

        await user.ModifyAsync(x => x.Nickname = nickname);
    }
}
