using Discord;
using Moq;
using QiQiBot.BotCommands;
using QiQiBot.Services;

namespace QiQiBot.UnitTests.BotCommands;

public class AdminChannelCommandsTests
{
    [Fact]
    public async Task ClanSetAchievementChannel_WithChannel_SetsChannelAndResponds()
    {
        var service = new Mock<IClanService>();
        var channel = new Mock<IChannel>();
        channel.SetupGet(x => x.Id).Returns(55UL);

        var context = new TestBotCommandContext
        {
            GuildId = 10,
            Options = [new BotCommandOption("channel", channel.Object)]
        };

        var command = new ClanSetAchievementChannel(service.Object);
        await command.Handle(context);

        service.Verify(x => x.SetAchievementChannel(10, 55), Times.Once);
        Assert.Equal("Channel for achievements has been set.", context.LastResponseText);
    }

    [Fact]
    public async Task ClanSetCitadelChannel_NoChannel_ClearsChannelAndResponds()
    {
        var service = new Mock<IClanService>();
        var context = new TestBotCommandContext { GuildId = 10, Options = [] };

        var command = new ClanSetCitadelChannel(service.Object);
        await command.Handle(context);

        service.Verify(x => x.SetCitadelChannel(10, null), Times.Once);
        Assert.Equal("Channel for citadel notifications has been cleared.", context.LastResponseText);
    }

    [Fact]
    public async Task ClanSetLeaveJoinChannel_WithChannel_SetsChannelAndResponds()
    {
        var service = new Mock<IClanService>();
        var channel = new Mock<IChannel>();
        channel.SetupGet(x => x.Id).Returns(66UL);

        var context = new TestBotCommandContext
        {
            GuildId = 10,
            Options = [new BotCommandOption("channel", channel.Object)]
        };

        var command = new ClanSetLeaveJoinChannel(service.Object);
        await command.Handle(context);

        service.Verify(x => x.SetLeaveJoinChannel(10, 66), Times.Once);
        Assert.Equal("Channel for leave and join events has been set.", context.LastResponseText);
    }

    [Fact]
    public async Task ClanSetWelcomeChannel_NoChannel_ClearsChannelAndResponds()
    {
        var service = new Mock<IClanService>();
        var context = new TestBotCommandContext { GuildId = 10, Options = [] };

        var command = new ClanSetWelcomeChannel(service.Object);
        await command.Handle(context);

        service.Verify(x => x.SetWelcomeChannel(10, null), Times.Once);
        Assert.Equal("Channel for welcome events has been cleared.", context.LastResponseText);
    }
}
