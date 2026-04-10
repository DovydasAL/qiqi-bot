using Discord.WebSocket;

namespace QiQiBot.BotCommands;

internal sealed class DiscordBotCommandContext : IBotCommandContext
{
    private readonly SocketSlashCommand _command;

    public DiscordBotCommandContext(SocketSlashCommand command)
    {
        _command = command;
        Options = _command.Data.Options
            .Select(option => new BotCommandOption(option.Name, option.Value))
            .ToList();
    }

    public string CommandName => _command.CommandName;
    public ulong? GuildId => _command.GuildId;
    public ulong UserId => _command.User.Id;
    public bool HasResponded => _command.HasResponded;
    public IReadOnlyList<BotCommandOption> Options { get; }

    public Task RespondAsync(string text, bool ephemeral = false)
        => _command.RespondAsync(text, ephemeral: ephemeral);

    public Task RespondWithFileAsync(Stream stream, string fileName, string? text = null)
        => _command.RespondWithFileAsync(stream, fileName, text);
}
