using Microsoft.Extensions.Options;
using QiQiBot.Models;
using QiQiBot.Services.RuneMetrics;
using static QiQiBot.Models.RuneMetricsProfileDTO;

namespace QiQiBot.UnitTests.Services.RuneMetrics;

public class RegexAchievementFilterTests
{
    [Fact]
    public void IsFiltered_WhenTextMatchesConfiguredTextPattern_ReturnsTrue()
    {
        var sut = CreateSut();
        var activity = CreateActivity("I gained 123XP in Attack.", "Some detail");

        var result = sut.IsFiltered(activity);

        Assert.True(result);
    }

    [Fact]
    public void IsFiltered_WhenTextDoesNotMatchAndDetailsMatchConfiguredDetailPattern_ReturnsTrue()
    {
        var sut = CreateSut();
        var activity = CreateActivity("Unrelated activity", "I am now level 98 in Strength.");

        var result = sut.IsFiltered(activity);

        Assert.True(result);
    }

    [Fact]
    public void IsFiltered_WhenTextIsEmpty_DoesNotFilterEvenIfDetailsWouldMatch()
    {
        var sut = CreateSut();
        var activity = CreateActivity(string.Empty, "I am now level 98 in Strength.");

        var result = sut.IsFiltered(activity);

        Assert.False(result);
    }

    [Fact]
    public void IsFiltered_WhenNoPatternMatches_ReturnsFalse()
    {
        var sut = CreateSut();
        var activity = CreateActivity("Just chatted with clan mates.", "No milestone details.");

        var result = sut.IsFiltered(activity);

        Assert.False(result);
    }

    private static RegexAchievementFilter CreateSut()
    {
        var options = Options.Create(AchievementFilterOptions.CreateDefault());
        return new RegexAchievementFilter(options);
    }

    private static RuneMetricsActivityDTO CreateActivity(string text, string details)
    {
        return new RuneMetricsActivityDTO
        {
            Date = "18-Jan-2026 23:52",
            Text = text,
            Details = details
        };
    }
}
