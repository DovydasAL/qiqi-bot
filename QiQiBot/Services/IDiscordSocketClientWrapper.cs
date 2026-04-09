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
}
