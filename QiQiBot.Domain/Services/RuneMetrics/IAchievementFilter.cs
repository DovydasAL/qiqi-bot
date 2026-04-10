using static QiQiBot.Models.RuneMetricsProfileDTO;

namespace QiQiBot.Services.RuneMetrics;

public interface IAchievementFilter
{
    bool IsFiltered(RuneMetricsActivityDTO activity);
}
