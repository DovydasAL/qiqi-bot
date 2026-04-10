using Moq;
using QiQiBot.BotCommands;
using QiQiBot.Services;

namespace QiQiBot.UnitTests.BotCommands;

public class AdminConfigurationCommandsTests
{
    [Fact]
    public async Task ClanRegister_InvalidName_RespondsValidationError()
    {
        var service = new Mock<IClanService>();
        var command = new ClanRegisterCommand(service.Object);
        var context = new TestBotCommandContext
        {
            GuildId = 42,
            Options = [new BotCommandOption("clan_name", "this-name-is-way-too-long-for-validation")]
        };

        await command.Handle(context);

        service.Verify(x => x.RegisterClan(It.IsAny<string>(), It.IsAny<ulong>()), Times.Never);
        Assert.Equal("Clan name cannot be empty and must be 20 characters or less.", context.LastResponseText);
    }

    [Fact]
    public async Task ClanRegister_ValidName_RegistersClan()
    {
        var service = new Mock<IClanService>();
        var command = new ClanRegisterCommand(service.Object);
        var context = new TestBotCommandContext
        {
            GuildId = 42,
            Options = [new BotCommandOption("clan_name", "My Clan")]
        };

        await command.Handle(context);

        service.Verify(x => x.RegisterClan("My Clan", 42), Times.Once);
        Assert.Equal("Clan My Clan has been registered for this server.", context.LastResponseText);
    }

    [Fact]
    public async Task ClanDebug_ValidOption_UpdatesDebugMode()
    {
        var service = new Mock<IClanService>();
        var command = new ClanDebugCommand(service.Object);
        var context = new TestBotCommandContext
        {
            GuildId = 42,
            Options = [new BotCommandOption("enabled", true)]
        };

        await command.Handle(context);

        service.Verify(x => x.SetDebugMode(42, true), Times.Once);
        Assert.Equal("Clan debug mode has been enabled.", context.LastResponseText);
    }

    [Fact]
    public async Task ClanSetCitadelReset_ValidInput_UpdatesResetTime()
    {
        var service = new Mock<IClanService>();
        var command = new ClanSetCitadelResetCommand(service.Object);
        var context = new TestBotCommandContext
        {
            GuildId = 99,
            Options =
            [
                new BotCommandOption("day", 2L),
                new BotCommandOption("time", "19:30")
            ]
        };

        await command.Handle(context);

        service.Verify(x => x.SetCitadelResetTime(99, 2, "19:30"), Times.Once);
        Assert.Equal("Citadel reset time has been set to Tuesday at 19:30.", context.LastResponseText);
    }
}
