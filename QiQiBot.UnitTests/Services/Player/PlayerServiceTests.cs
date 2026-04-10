using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using QiQiBot.Models;
using QiQiBot.Services;

namespace QiQiBot.UnitTests.Services.Player;

public class PlayerServiceTests
{
    [Fact]
    public async Task UpdatePlayersFromRuneMetrics_WhenActivitiesAreEmpty_DoesNotThrowAndStillUpdatesScrapeTime()
    {
        await using var dbContext = CreateDbContext();
        dbContext.Players.Add(new QiQiBot.Models.Player { Name = "Alice" });
        await dbContext.SaveChangesAsync();

        var logger = new Mock<ILogger<PlayerService>>();
        var sut = new PlayerService(dbContext, logger.Object);
        var scrapeTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        await sut.UpdatePlayersFromRuneMetrics(
            ["Alice"],
            [
                new RuneMetricsProfileDTO
                {
                    Name = "Alice",
                    ScrapedDate = scrapeTime,
                    Activities = []
                }
            ]);

        var updatedPlayer = await dbContext.Players.SingleAsync(x => x.Name == "Alice");
        Assert.Equal(scrapeTime, updatedPlayer.LastScrapedRuneMetricsProfile);
        Assert.Null(updatedPlayer.MostRecentRuneMetricsEvent);
    }

    private static ClanContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ClanContext>()
            .UseInMemoryDatabase($"player-service-{Guid.NewGuid()}")
            .Options;

        return new ClanContext(options);
    }
}
