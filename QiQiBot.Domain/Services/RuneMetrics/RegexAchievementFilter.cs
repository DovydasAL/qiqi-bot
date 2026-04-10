using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using static QiQiBot.Models.RuneMetricsProfileDTO;

namespace QiQiBot.Services.RuneMetrics;

public sealed class RegexAchievementFilter : IAchievementFilter
{
    private readonly Regex[] _textRegexes;
    private readonly Regex[] _detailRegexes;

    public RegexAchievementFilter(IOptions<AchievementFilterOptions> options)
    {
        _textRegexes = (options.Value.TextPatterns ?? [])
            .Select(pattern => new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
            .ToArray();

        _detailRegexes = (options.Value.DetailPatterns ?? [])
            .Select(pattern => new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
            .ToArray();
    }

    public bool IsFiltered(RuneMetricsActivityDTO activity)
    {
        if (activity == null || string.IsNullOrEmpty(activity.Text))
        {
            return false;
        }

        return _textRegexes.Any(regex => regex.IsMatch(activity.Text))
            || _detailRegexes.Any(regex => regex.IsMatch(activity.Details));
    }
}
