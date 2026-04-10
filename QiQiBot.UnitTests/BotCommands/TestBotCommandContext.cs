using QiQiBot.BotCommands;

namespace QiQiBot.UnitTests.BotCommands;

internal sealed class TestBotCommandContext : IBotCommandContext
{
    public string CommandName { get; init; } = string.Empty;
    public ulong? GuildId { get; init; }
    public ulong UserId { get; init; }
    public bool HasResponded { get; private set; }
    public IReadOnlyList<BotCommandOption> Options { get; init; } = [];

    public string? LastResponseText { get; private set; }
    public bool LastResponseEphemeral { get; private set; }
    public bool RespondWithFileCalled { get; private set; }
    public string? LastFileName { get; private set; }
    public string? LastFileText { get; private set; }

    public Task RespondAsync(string text, bool ephemeral = false)
    {
        HasResponded = true;
        LastResponseText = text;
        LastResponseEphemeral = ephemeral;
        return Task.CompletedTask;
    }

    public async Task RespondWithFileAsync(Stream stream, string fileName, string? text = null)
    {
        HasResponded = true;
        RespondWithFileCalled = true;
        LastFileName = fileName;

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        using var reader = new StreamReader(stream, leaveOpen: true);
        LastFileText = await reader.ReadToEndAsync();

        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        LastResponseText = text;
    }
}
