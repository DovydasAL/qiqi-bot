using Discord;
using Discord.WebSocket;

namespace QiQiBot.Services;

public interface IDiscordSocketClientWrapper
{
    event Func<Task>? Ready;
    event Func<SocketSlashCommand, Task>? SlashCommandExecuted;
    event Func<SocketGuildUser, Task>? UserJoined;
    event Func<LogMessage, Task>? Log;

    Task LoginAsync(TokenType tokenType, string token);
    Task StartAsync();
    Task StopAsync();
    Task BulkOverwriteGlobalApplicationCommandsAsync(ApplicationCommandProperties[] properties);
    SocketGuild? GetGuild(ulong id);

    IReadOnlyList<DiscordGuildUserInfo>? GetGuildUsers(ulong guildId);
    DiscordGuildUserInfo? GetGuildUser(ulong guildId, ulong userId);
    Task TrySetGuildUserNicknameAsync(ulong guildId, ulong userId, string nickname);
}

public sealed record DiscordGuildUserInfo(ulong Id, string Username, string DisplayName, bool IsBot);