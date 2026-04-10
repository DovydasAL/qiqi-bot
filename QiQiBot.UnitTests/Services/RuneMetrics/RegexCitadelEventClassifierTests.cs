using Microsoft.Extensions.Options;
using QiQiBot.Services.RuneMetrics;
using static QiQiBot.Models.RuneMetricsProfileDTO;

namespace QiQiBot.UnitTests.Services.RuneMetrics;

public class RegexCitadelEventClassifierTests
{
    [Fact]
    public void Classify_WhenCapPatternMatches_ReturnsCapped()
    {
        var sut = CreateSut();
        var activity = CreateActivity("I capped at my clan citadel this week.");

        var result = sut.Classify(activity);

        Assert.Equal(CitadelEventType.Capped, result);
    }

    [Fact]
    public void Classify_WhenVisitPatternMatches_ReturnsVisited()
    {
        var sut = CreateSut();
        var activity = CreateActivity("I Visited my Clan Citadel today.");

        var result = sut.Classify(activity);

        Assert.Equal(CitadelEventType.Visited, result);
    }

    [Fact]
    public void Classify_WhenBothPatternsMatch_ReturnsCapped()
    {
        var sut = CreateSut();
        var activity = CreateActivity("Visited my Clan Citadel and capped at my clan citadel.");

        var result = sut.Classify(activity);

        Assert.Equal(CitadelEventType.Capped, result);
    }

    [Fact]
    public void Classify_WhenNoPatternMatches_ReturnsNull()
    {
        var sut = CreateSut();
        var activity = CreateActivity("Did a clue scroll.");

        var result = sut.Classify(activity);

        Assert.Null(result);
    }

    [Fact]
    public void Classify_WhenTextIsEmpty_ReturnsNull()
    {
        var sut = CreateSut();
        var activity = CreateActivity(string.Empty);

        var result = sut.Classify(activity);

        Assert.Null(result);
    }

    private static RegexCitadelEventClassifier CreateSut()
    {
        var options = Options.Create(CitadelPatternOptions.CreateDefault());
        return new RegexCitadelEventClassifier(options);
    }

    private static RuneMetricsActivityDTO CreateActivity(string text)
    {
        return new RuneMetricsActivityDTO
        {
            Date = "18-Jan-2026 23:52",
            Text = text,
            Details = "detail"
        };
    }
}
