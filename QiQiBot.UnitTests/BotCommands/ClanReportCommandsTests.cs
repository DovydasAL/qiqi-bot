using Moq;
using QiQiBot.BotCommands;
using QiQiBot.Exceptions;
using QiQiBot.Models;
using QiQiBot.Services;

namespace QiQiBot.UnitTests.BotCommands;

public class ClanReportCommandsTests
{
    [Fact]
    public async Task ClanActivity_WhenNoClanRegistered_RespondsWithRegistrationMessage()
    {
        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetClanAsync(100)).ThrowsAsync(new NoClanRegisteredException(100));

        var command = new ClanActivityCommand(clanService.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.Equal("No clan has been set for this server. Use `/clan-register` to set the clan for this server.", context.LastResponseText);
    }

    [Fact]
    public async Task ClanActivity_WhenClanExists_ReturnsCsvFile()
    {
        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetClanAsync(100)).ReturnsAsync(new Clan { Id = 7, Name = "A" });
        clanService.Setup(x => x.GetClanMembers(7)).ReturnsAsync(
        [
            new Player { Name = "Alice", LastClanExperienceUpdate = new DateTime(2026, 01, 03) },
            new Player { Name = "Bob", MostRecentRuneMetricsEvent = new DateTime(2026, 01, 02) }
        ]);

        var command = new ClanActivityCommand(clanService.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.True(context.RespondWithFileCalled);
        Assert.NotNull(context.LastFileText);
        Assert.Contains("Name,Last Active", context.LastFileText);
        Assert.Contains("Alice", context.LastFileText);
        Assert.Contains("Bob", context.LastFileText);
    }

    [Fact]
    public async Task ClanActivity_WhenOnlyRuneMetricsPresent_CoversAlternateActivityBranch()
    {
        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetClanAsync(100)).ReturnsAsync(new Clan { Id = 7, Name = "A" });
        clanService.Setup(x => x.GetClanMembers(7)).ReturnsAsync(
        [
            new Player { Name = "NoActivity" },
            new Player { Name = "RuneMetricsOnly", MostRecentRuneMetricsEvent = new DateTime(2026, 01, 01) }
        ]);

        var command = new ClanActivityCommand(clanService.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.Contains("RuneMetricsOnly", context.LastFileText);
        Assert.Contains("NoActivity,Unknown", context.LastFileText);
    }

    [Fact]
    public async Task ClanActivity_WhenBothActivitySourcesPresent_UsesMostRecentDate()
    {
        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetClanAsync(100)).ReturnsAsync(new Clan { Id = 7, Name = "A" });
        clanService.Setup(x => x.GetClanMembers(7)).ReturnsAsync(
        [
            new Player
            {
                Name = "DualDates",
                LastClanExperienceUpdate = new DateTime(2026, 01, 01),
                MostRecentRuneMetricsEvent = new DateTime(2026, 01, 05)
            }
        ]);

        var command = new ClanActivityCommand(clanService.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.Contains("DualDates", context.LastFileText);
    }

    [Fact]
    public async Task ClanCapped_WhenNoClanRegistered_RespondsWithRegistrationMessage()
    {
        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetGuild(100)).ThrowsAsync(new NoClanRegisteredException(100));

        var command = new ClanCappedCommand(clanService.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.Equal("No clan has been set for this server. Use `/clan-register` to set the clan for this server.", context.LastResponseText);
    }

    [Fact]
    public async Task ClanCapped_WhenGuildHasNoClan_RespondsWithRegistrationMessage()
    {
        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetGuild(100)).ReturnsAsync(new Guild { GuildId = 100, ClanId = null });

        var command = new ClanCappedCommand(clanService.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.Equal("No clan has been set for this server. Use `/clan-register` to set the clan for this server.", context.LastResponseText);
    }

    [Fact]
    public async Task ClanCapped_WhenResetNotConfigured_RespondsWithResetMessage()
    {
        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetGuild(100)).ReturnsAsync(new Guild { GuildId = 100, ClanId = 55, CapResetDay = null, CapResetTime = null });

        var command = new ClanCappedCommand(clanService.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.Equal("Cap reset day and time have not been set for this server. Use `/clan-citadel-reset` to set the cap reset day and time for this server.", context.LastResponseText);
    }

    [Fact]
    public async Task ClanCapped_WhenInvalidResetDay_RespondsValidationError()
    {
        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetGuild(100)).ReturnsAsync(new Guild
        {
            GuildId = 100,
            ClanId = 55,
            CapResetDay = 99,
            CapResetTime = "12:00"
        });

        var command = new ClanCappedCommand(clanService.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.Equal("Cap reset day is invalid for this server. Use `/clan-citadel-reset` to set the cap reset day and time again.", context.LastResponseText);
    }

    [Fact]
    public async Task ClanCapped_WhenInvalidResetTime_RespondsValidationError()
    {
        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetGuild(100)).ReturnsAsync(new Guild
        {
            GuildId = 100,
            ClanId = 55,
            CapResetDay = 2,
            CapResetTime = "not-a-time"
        });

        var command = new ClanCappedCommand(clanService.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.Equal("Cap reset time is invalid for this server. Use `/clan-citadel-reset` to set the cap reset day and time again.", context.LastResponseText);
    }

    [Fact]
    public async Task ClanCapped_WhenConfigured_ReturnsCsvFile()
    {
        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetGuild(100)).ReturnsAsync(new Guild
        {
            GuildId = 100,
            ClanId = 55,
            CapResetDay = (long)DateTime.UtcNow.DayOfWeek,
            CapResetTime = "00:00"
        });

        clanService.Setup(x => x.GetClanMembers(55)).ReturnsAsync(
        [
            new Player { Name = "Capper", LastCapped = DateTime.UtcNow },
            new Player { Name = "NotCapper", LastCapped = null }
        ]);

        var command = new ClanCappedCommand(clanService.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.True(context.RespondWithFileCalled);
        Assert.Contains("Name,Capped", context.LastFileText);
        Assert.Contains("Capper", context.LastFileText);
        Assert.DoesNotContain("NotCapper", context.LastFileText);
    }

    [Fact]
    public async Task ClanCapped_WhenResetTimeInFuture_CoversPreviousWeekBranch()
    {
        var future = DateTime.UtcNow.AddHours(2).ToString("HH:mm");
        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetGuild(100)).ReturnsAsync(new Guild
        {
            GuildId = 100,
            ClanId = 55,
            CapResetDay = (long)DateTime.UtcNow.DayOfWeek,
            CapResetTime = future
        });

        clanService.Setup(x => x.GetClanMembers(55)).ReturnsAsync([new Player { Name = "Capper", LastCapped = DateTime.UtcNow }]);

        var command = new ClanCappedCommand(clanService.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.True(context.RespondWithFileCalled);
        Assert.Contains("Capper", context.LastFileText);
    }

    [Fact]
    public async Task ClanRsnAudit_WhenGuildNotInCache_RespondsWithCacheMessage()
    {
        var rsnService = new Mock<IRsnService>();
        var clanService = new Mock<IClanService>();
        var client = new Mock<IDiscordSocketClientWrapper>();
        client.Setup(x => x.GetGuildUsers(100)).Returns((IReadOnlyList<DiscordGuildUserInfo>?)null);

        var command = new ClanRsnAuditCommand(rsnService.Object, clanService.Object, client.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.Equal("Could not find this Discord server in the bot cache.", context.LastResponseText);
    }

    [Fact]
    public async Task ClanRsnAudit_NoClanConfigured_IncludesNoClanMessage()
    {
        var rsnService = new Mock<IRsnService>();
        rsnService.Setup(x => x.GetRsnsAsync(100)).ReturnsAsync(new Dictionary<ulong, string> { [2] = "Gamma" });

        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetClanAsync(100)).ThrowsAsync(new NoClanRegisteredException(100));

        var client = new Mock<IDiscordSocketClientWrapper>();
        client.Setup(x => x.GetGuildUsers(100)).Returns(
        [
            new DiscordGuildUserInfo(1, "User1", "Display1", false),
            new DiscordGuildUserInfo(2, "User2", "Display2", false)
        ]);

        var command = new ClanRsnAuditCommand(rsnService.Object, clanService.Object, client.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.True(context.RespondWithFileCalled);
        Assert.Contains("- User1 (1)", context.LastFileText);
        Assert.Contains("- No clan is configured for this server.", context.LastFileText);
    }

    [Fact]
    public async Task ClanRsnAudit_ClanConfigured_WithMismatch_UsesGuildUserWhenAvailable()
    {
        var rsnService = new Mock<IRsnService>();
        rsnService.Setup(x => x.GetRsnsAsync(100)).ReturnsAsync(new Dictionary<ulong, string>
        {
            [5] = "OutOfClan",
            [6] = "UnknownUserRsn"
        });

        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetClanAsync(100)).ReturnsAsync(new Clan { Id = 7, Name = "A" });
        clanService.Setup(x => x.GetClanMembers(7)).ReturnsAsync([new Player { Name = "InClan" }]);

        var client = new Mock<IDiscordSocketClientWrapper>();
        client.Setup(x => x.GetGuildUsers(100)).Returns([new DiscordGuildUserInfo(1, "DiscordOne", "DiscordOne", false)]);
        client.Setup(x => x.GetGuildUser(100, 5)).Returns(new DiscordGuildUserInfo(5, "KnownUser", "KnownUser", false));
        client.Setup(x => x.GetGuildUser(100, 6)).Returns((DiscordGuildUserInfo?)null);

        var command = new ClanRsnAuditCommand(rsnService.Object, clanService.Object, client.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.Contains("- OutOfClan (Discord: KnownUser, 5)", context.LastFileText);
        Assert.Contains("- UnknownUserRsn (Discord User ID: 6)", context.LastFileText);
    }

    [Fact]
    public async Task ClanRsnAudit_ClanConfigured_NoMismatches_IncludesNone()
    {
        var rsnService = new Mock<IRsnService>();
        rsnService.Setup(x => x.GetRsnsAsync(100)).ReturnsAsync(new Dictionary<ulong, string> { [10] = "InClan" });

        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetClanAsync(100)).ReturnsAsync(new Clan { Id = 7, Name = "A" });
        clanService.Setup(x => x.GetClanMembers(7)).ReturnsAsync([new Player { Name = "InClan" }]);

        var client = new Mock<IDiscordSocketClientWrapper>();
        client.Setup(x => x.GetGuildUsers(100)).Returns([new DiscordGuildUserInfo(10, "DiscordTen", "DiscordTen", false)]);

        var command = new ClanRsnAuditCommand(rsnService.Object, clanService.Object, client.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.Contains("- None", context.LastFileText);
    }

    [Fact]
    public async Task ClanRsnAudit_BotUsersAreExcludedFromMissingRsnList()
    {
        var rsnService = new Mock<IRsnService>();
        rsnService.Setup(x => x.GetRsnsAsync(100)).ReturnsAsync(new Dictionary<ulong, string>());

        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetClanAsync(100)).ThrowsAsync(new NoClanRegisteredException(100));

        var client = new Mock<IDiscordSocketClientWrapper>();
        client.Setup(x => x.GetGuildUsers(100)).Returns(
        [
            new DiscordGuildUserInfo(1, "HumanUser", "HumanUser", false),
            new DiscordGuildUserInfo(2, "BotUser", "BotUser", true)
        ]);

        var command = new ClanRsnAuditCommand(rsnService.Object, clanService.Object, client.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        Assert.Contains("- HumanUser (1)", context.LastFileText);
        Assert.DoesNotContain("BotUser", context.LastFileText);
    }

    [Fact]
    public async Task ClanRsnAudit_MissingRsnUsers_AreSortedByDisplayName()
    {
        var rsnService = new Mock<IRsnService>();
        rsnService.Setup(x => x.GetRsnsAsync(100)).ReturnsAsync(new Dictionary<ulong, string>());

        var clanService = new Mock<IClanService>();
        clanService.Setup(x => x.GetClanAsync(100)).ThrowsAsync(new NoClanRegisteredException(100));

        var client = new Mock<IDiscordSocketClientWrapper>();
        client.Setup(x => x.GetGuildUsers(100)).Returns(
        [
            new DiscordGuildUserInfo(1, "UserB", "Zulu", false),
            new DiscordGuildUserInfo(2, "UserA", "Alpha", false)
        ]);

        var command = new ClanRsnAuditCommand(rsnService.Object, clanService.Object, client.Object);
        var context = new TestBotCommandContext { GuildId = 100 };

        await command.Handle(context);

        var alphaIndex = context.LastFileText!.IndexOf("- UserA (2)", StringComparison.Ordinal);
        var zuluIndex = context.LastFileText.IndexOf("- UserB (1)", StringComparison.Ordinal);
        Assert.True(alphaIndex >= 0 && zuluIndex > alphaIndex);
    }
}
