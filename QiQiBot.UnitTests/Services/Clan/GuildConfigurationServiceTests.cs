using Microsoft.EntityFrameworkCore;
using QiQiBot.Exceptions;
using QiQiBot.Models;
using QiQiBot.Services;

namespace QiQiBot.UnitTests.Services.Clan;

public class GuildConfigurationServiceTests
{
    [Fact]
    public async Task SetAchievementChannel_WhenGuildDoesNotExist_ThrowsGuildNotFoundException()
    {
        await using var dbContext = CreateDbContext();
        var sut = new GuildConfigurationService(dbContext);

        await Assert.ThrowsAsync<GuildNotFoundException>(() => sut.SetAchievementChannel(999, 123));
    }

    [Fact]
    public async Task SetCitadelResetTime_WhenTimeInvalid_ThrowsInvalidResetTimeException()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Guilds.Add(new Guild { GuildId = 42 });
        await dbContext.SaveChangesAsync();

        var sut = new GuildConfigurationService(dbContext);

        await Assert.ThrowsAsync<InvalidResetTimeException>(() => sut.SetCitadelResetTime(42, 1, "99:99"));
    }

    private static ClanContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ClanContext>()
            .UseInMemoryDatabase($"guild-config-{Guid.NewGuid()}")
            .Options;

        return new ClanContext(options);
    }
}
