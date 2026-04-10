using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using static QiQiBot.Models.RuneMetricsProfileDTO;

namespace QiQiBot.Services.RuneMetrics;

public sealed class RegexCitadelEventClassifier : ICitadelEventClassifier
{
    private readonly Regex _visitRegex;
    private readonly Regex _capRegex;

    public RegexCitadelEventClassifier(IOptions<CitadelPatternOptions> options)
    {
        var configuredOptions = options.Value;

        if (string.IsNullOrWhiteSpace(configuredOptions.VisitPattern))
        {
            throw new InvalidOperationException("Citadel visit pattern must be configured.");
        }

        if (string.IsNullOrWhiteSpace(configuredOptions.CapPattern))
        {
            throw new InvalidOperationException("Citadel cap pattern must be configured.");
        }

        _visitRegex = new Regex(configuredOptions.VisitPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        _capRegex = new Regex(configuredOptions.CapPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    public CitadelEventType? Classify(RuneMetricsActivityDTO activity)
    {
        if (activity == null || string.IsNullOrWhiteSpace(activity.Text))
        {
            return null;
        }

        if (_capRegex.IsMatch(activity.Text))
        {
            return CitadelEventType.Capped;
        }

        if (_visitRegex.IsMatch(activity.Text))
        {
            return CitadelEventType.Visited;
        }

        return null;
    }
}
