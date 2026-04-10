namespace QiQiBot.Services.RuneMetrics;

public sealed class CitadelPatternOptions
{
    public string VisitPattern { get; set; } = string.Empty;
    public string CapPattern { get; set; } = string.Empty;

    public static CitadelPatternOptions CreateDefault() => new()
    {
        VisitPattern = @".*Visited my Clan Citadel.*",
        CapPattern = @".*capped at my clan citadel.*"
    };
}
