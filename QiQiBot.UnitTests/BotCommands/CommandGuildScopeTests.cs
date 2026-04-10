using Moq;
using QiQiBot.BotCommands;
using QiQiBot.Services;

namespace QiQiBot.UnitTests.BotCommands;

public class CommandGuildScopeTests
{
    public static IEnumerable<object[]> CommandFactories()
    {
        yield return [new Func<IBotCommandContext, Task>(ctx => new ClanActivityCommand(Mock.Of<IClanService>()).Handle(ctx)), false];
        yield return [new Func<IBotCommandContext, Task>(ctx => new ClanCappedCommand(Mock.Of<IClanService>()).Handle(ctx)), false];
        yield return [new Func<IBotCommandContext, Task>(ctx => new ClanDebugCommand(Mock.Of<IClanService>()).Handle(ctx)), false];
        yield return [new Func<IBotCommandContext, Task>(ctx => new ClanRegisterCommand(Mock.Of<IClanService>()).Handle(ctx)), false];
        yield return [new Func<IBotCommandContext, Task>(ctx => new ClanRsnAuditCommand(Mock.Of<IRsnService>(), Mock.Of<IClanService>(), Mock.Of<IDiscordSocketClientWrapper>()).Handle(ctx)), false];
        yield return [new Func<IBotCommandContext, Task>(ctx => new ClanSetAchievementChannel(Mock.Of<IClanService>()).Handle(ctx)), false];
        yield return [new Func<IBotCommandContext, Task>(ctx => new ClanSetCitadelChannel(Mock.Of<IClanService>()).Handle(ctx)), false];
        yield return [new Func<IBotCommandContext, Task>(ctx => new ClanSetCitadelResetCommand(Mock.Of<IClanService>()).Handle(ctx)), false];
        yield return [new Func<IBotCommandContext, Task>(ctx => new ClanSetLeaveJoinChannel(Mock.Of<IClanService>()).Handle(ctx)), false];
        yield return [new Func<IBotCommandContext, Task>(ctx => new ClanSetWelcomeChannel(Mock.Of<IClanService>()).Handle(ctx)), false];
        yield return [new Func<IBotCommandContext, Task>(ctx => new RsnCommand(Mock.Of<IRsnService>(), Mock.Of<IDiscordSocketClientWrapper>()).Handle(ctx)), false];
        yield return [new Func<IBotCommandContext, Task>(ctx => new RsnSetCommand(Mock.Of<IRsnService>(), Mock.Of<IDiscordSocketClientWrapper>()).Handle(ctx)), true];
    }

    [Theory]
    [MemberData(nameof(CommandFactories))]
    public async Task Handle_WhenGuildIdMissing_RespondsWithGuildOnlyMessage(
        Func<IBotCommandContext, Task> handle,
        bool expectedEphemeral)
    {
        var context = new FakeBotCommandContext { GuildId = null };

        await handle(context);

        Assert.Equal("This command can only be used in a server.", context.LastResponseText);
        Assert.Equal(expectedEphemeral, context.LastResponseEphemeral);
        Assert.False(context.RespondWithFileCalled);
    }

    private sealed class FakeBotCommandContext : IBotCommandContext
    {
        public string CommandName { get; init; } = string.Empty;
        public ulong? GuildId { get; init; }
        public ulong UserId { get; init; }
        public bool HasResponded { get; private set; }
        public IReadOnlyList<BotCommandOption> Options { get; init; } = [];

        public string? LastResponseText { get; private set; }
        public bool LastResponseEphemeral { get; private set; }
        public bool RespondWithFileCalled { get; private set; }

        public Task RespondAsync(string text, bool ephemeral = false)
        {
            HasResponded = true;
            LastResponseText = text;
            LastResponseEphemeral = ephemeral;
            return Task.CompletedTask;
        }

        public Task RespondWithFileAsync(Stream stream, string fileName, string? text = null)
        {
            HasResponded = true;
            RespondWithFileCalled = true;
            return Task.CompletedTask;
        }
    }
}
