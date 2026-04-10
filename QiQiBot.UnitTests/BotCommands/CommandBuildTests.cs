using QiQiBot.BotCommands;

namespace QiQiBot.UnitTests.BotCommands;

public class CommandBuildTests
{
    [Fact]
    public void BuildCommand_ReturnsProperties_ForAllCommands()
    {
        Assert.NotNull(ClanActivityCommand.BuildCommand());
        Assert.NotNull(ClanCappedCommand.BuildCommand());
        Assert.NotNull(ClanDebugCommand.BuildCommand());
        Assert.NotNull(ClanRegisterCommand.BuildCommand());
        Assert.NotNull(ClanRsnAuditCommand.BuildCommand());
        Assert.NotNull(ClanSetAchievementChannel.BuildCommand());
        Assert.NotNull(ClanSetCitadelChannel.BuildCommand());
        Assert.NotNull(ClanSetCitadelResetCommand.BuildCommand());
        Assert.NotNull(ClanSetLeaveJoinChannel.BuildCommand());
        Assert.NotNull(ClanSetWelcomeChannel.BuildCommand());
        Assert.NotNull(RsnCommand.BuildCommand());
        Assert.NotNull(RsnSetCommand.BuildCommand());
    }
}
