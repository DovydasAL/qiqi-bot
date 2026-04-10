using Discord;
using Moq;
using QiQiBot.BotCommands;
using QiQiBot.Services;

namespace QiQiBot.UnitTests.BotCommands;

public class RsnCommandsTests
{
    [Fact]
    public async Task RsnCommand_InvalidName_RespondsValidationError()
    {
        var rsnService = new Mock<IRsnService>();
        var client = new Mock<IDiscordSocketClientWrapper>();
        var command = new RsnCommand(rsnService.Object, client.Object);
        var context = new TestBotCommandContext
        {
            GuildId = 1,
            UserId = 2,
            Options = [new BotCommandOption("name", "")]
        };

        await command.Handle(context);

        Assert.Equal("RuneScape names must be between 1 and 12 characters.", context.LastResponseText);
        Assert.True(context.LastResponseEphemeral);
        rsnService.Verify(x => x.SetRsnAsync(It.IsAny<ulong>(), It.IsAny<ulong>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RsnCommand_FirstTime_SetAndResponds()
    {
        var rsnService = new Mock<IRsnService>();
        rsnService.Setup(x => x.GetRsnAsync(1, 2)).ReturnsAsync((string?)null);
        var client = new Mock<IDiscordSocketClientWrapper>();
        client.Setup(x => x.TrySetGuildUserNicknameAsync(1, 2, "Zephyr")).Returns(Task.CompletedTask);

        var command = new RsnCommand(rsnService.Object, client.Object);
        var context = new TestBotCommandContext
        {
            GuildId = 1,
            UserId = 2,
            Options = [new BotCommandOption("name", "Zephyr")]
        };

        await command.Handle(context);

        rsnService.Verify(x => x.SetRsnAsync(1, 2, "Zephyr"), Times.Once);
        client.Verify(x => x.TrySetGuildUserNicknameAsync(1, 2, "Zephyr"), Times.Once);
        Assert.Equal("Your RuneScape name has been set to Zephyr. Your Discord nickname has also been changed for this server.", context.LastResponseText);
    }

    [Fact]
    public async Task RsnCommand_SameName_RespondsAlreadySet()
    {
        var rsnService = new Mock<IRsnService>();
        rsnService.Setup(x => x.GetRsnAsync(1, 2)).ReturnsAsync("Zephyr");
        var client = new Mock<IDiscordSocketClientWrapper>();
        client.Setup(x => x.TrySetGuildUserNicknameAsync(1, 2, "Zephyr")).Returns(Task.CompletedTask);

        var command = new RsnCommand(rsnService.Object, client.Object);
        var context = new TestBotCommandContext
        {
            GuildId = 1,
            UserId = 2,
            Options = [new BotCommandOption("name", "Zephyr")]
        };

        await command.Handle(context);

        Assert.Equal("Your RuneScape name is already set to Zephyr.", context.LastResponseText);
        Assert.True(context.LastResponseEphemeral);
    }

    [Fact]
    public async Task RsnCommand_DifferentName_RespondsUpdated()
    {
        var rsnService = new Mock<IRsnService>();
        rsnService.Setup(x => x.GetRsnAsync(1, 2)).ReturnsAsync("OldName");
        var client = new Mock<IDiscordSocketClientWrapper>();
        client.Setup(x => x.TrySetGuildUserNicknameAsync(1, 2, "NewName")).Returns(Task.CompletedTask);

        var command = new RsnCommand(rsnService.Object, client.Object);
        var context = new TestBotCommandContext
        {
            GuildId = 1,
            UserId = 2,
            Options = [new BotCommandOption("name", "NewName")]
        };

        await command.Handle(context);

        Assert.Equal("Updated your RuneScape name from OldName to NewName. Your Discord nickname has also been changed for this server.", context.LastResponseText);
        Assert.True(context.LastResponseEphemeral);
    }

    [Fact]
    public async Task RsnSetCommand_InvalidTargetUser_RespondsValidationError()
    {
        var rsnService = new Mock<IRsnService>();
        var client = new Mock<IDiscordSocketClientWrapper>();
        var command = new RsnSetCommand(rsnService.Object, client.Object);
        var context = new TestBotCommandContext
        {
            GuildId = 1,
            Options =
            [
                new BotCommandOption("user", null),
                new BotCommandOption("name", "Valid")
            ]
        };

        await command.Handle(context);

        Assert.Equal("Please provide a valid server member.", context.LastResponseText);
        Assert.True(context.LastResponseEphemeral);
    }

    [Fact]
    public async Task RsnSetCommand_InvalidName_RespondsValidationError()
    {
        var rsnService = new Mock<IRsnService>();
        var client = new Mock<IDiscordSocketClientWrapper>();

        var guildUser = new Mock<IGuildUser>();
        guildUser.SetupGet(x => x.Id).Returns(5);
        guildUser.SetupGet(x => x.Mention).Returns("<@5>");

        var command = new RsnSetCommand(rsnService.Object, client.Object);
        var context = new TestBotCommandContext
        {
            GuildId = 1,
            Options =
            [
                new BotCommandOption("user", guildUser.Object),
                new BotCommandOption("name", "")
            ]
        };

        await command.Handle(context);

        Assert.Equal("RuneScape names must be between 1 and 12 characters.", context.LastResponseText);
        Assert.True(context.LastResponseEphemeral);
    }

    [Fact]
    public async Task RsnSetCommand_ValidInput_UpdatesRsnAndResponds()
    {
        var rsnService = new Mock<IRsnService>();
        rsnService.Setup(x => x.GetRsnAsync(1, 5)).ReturnsAsync("OldName");

        var client = new Mock<IDiscordSocketClientWrapper>();
        client.Setup(x => x.TrySetGuildUserNicknameAsync(1, 5, "NewName")).Returns(Task.CompletedTask);

        var guildUser = new Mock<IGuildUser>();
        guildUser.SetupGet(x => x.Id).Returns(5);
        guildUser.SetupGet(x => x.Mention).Returns("<@5>");

        var command = new RsnSetCommand(rsnService.Object, client.Object);
        var context = new TestBotCommandContext
        {
            GuildId = 1,
            Options =
            [
                new BotCommandOption("user", guildUser.Object),
                new BotCommandOption("name", "NewName")
            ]
        };

        await command.Handle(context);

        rsnService.Verify(x => x.SetRsnAsync(1, 5, "NewName"), Times.Once);
        client.Verify(x => x.TrySetGuildUserNicknameAsync(1, 5, "NewName"), Times.Once);
        Assert.Equal("Updated <@5>'s RuneScape name from OldName to NewName. Their Discord nickname has also been changed for this server.", context.LastResponseText);
        Assert.True(context.LastResponseEphemeral);
    }

    [Fact]
    public async Task RsnSetCommand_FirstTime_RespondsSetMessage()
    {
        var rsnService = new Mock<IRsnService>();
        rsnService.Setup(x => x.GetRsnAsync(1, 5)).ReturnsAsync((string?)null);

        var client = new Mock<IDiscordSocketClientWrapper>();
        client.Setup(x => x.TrySetGuildUserNicknameAsync(1, 5, "NewName")).Returns(Task.CompletedTask);

        var guildUser = new Mock<IGuildUser>();
        guildUser.SetupGet(x => x.Id).Returns(5);
        guildUser.SetupGet(x => x.Mention).Returns("<@5>");

        var command = new RsnSetCommand(rsnService.Object, client.Object);
        var context = new TestBotCommandContext
        {
            GuildId = 1,
            Options =
            [
                new BotCommandOption("user", guildUser.Object),
                new BotCommandOption("name", "NewName")
            ]
        };

        await command.Handle(context);

        Assert.Equal("Set <@5>'s RuneScape name to NewName. Their Discord nickname has also been changed for this server.", context.LastResponseText);
    }

    [Fact]
    public async Task RsnSetCommand_SameName_RespondsAlreadySetMessage()
    {
        var rsnService = new Mock<IRsnService>();
        rsnService.Setup(x => x.GetRsnAsync(1, 5)).ReturnsAsync("NewName");

        var client = new Mock<IDiscordSocketClientWrapper>();
        client.Setup(x => x.TrySetGuildUserNicknameAsync(1, 5, "NewName")).Returns(Task.CompletedTask);

        var guildUser = new Mock<IGuildUser>();
        guildUser.SetupGet(x => x.Id).Returns(5);
        guildUser.SetupGet(x => x.Mention).Returns("<@5>");

        var command = new RsnSetCommand(rsnService.Object, client.Object);
        var context = new TestBotCommandContext
        {
            GuildId = 1,
            Options =
            [
                new BotCommandOption("user", guildUser.Object),
                new BotCommandOption("name", "NewName")
            ]
        };

        await command.Handle(context);

        Assert.Equal("<@5>'s RuneScape name is already set to NewName.", context.LastResponseText);
    }
}
