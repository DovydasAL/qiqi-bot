using System.IO;

namespace QiQiBot.BotCommands;

public interface IBotCommandContext
{
    string CommandName { get; }
    ulong? GuildId { get; }
    ulong UserId { get; }
    bool HasResponded { get; }
    IReadOnlyList<BotCommandOption> Options { get; }

    Task RespondAsync(string text, bool ephemeral = false);
    Task RespondWithFileAsync(Stream stream, string fileName, string? text = null);
}

public sealed record BotCommandOption(string Name, object? Value);
