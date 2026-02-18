using QiQiBot.Models;

namespace QiQiBot.Services
{
    public interface IAchievementService
    {
        Task ProcessAchievementsAsync(List<RuneMetricsProfileDTO> profiles, CancellationToken cancellationToken);
    }
}
