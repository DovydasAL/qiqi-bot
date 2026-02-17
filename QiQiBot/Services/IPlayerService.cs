using QiQiBot.Models;

namespace QiQiBot.Services
{
    public interface IPlayerService
    {
        Task UpdatePlayersFromRuneMetrics(List<string> names, List<RuneMetricsProfileDTO> profiles);
        Task<List<Player>> GetLeastRecentlyScrapedMembers(int n, TimeSpan olderThan);
        Task<List<Player>> GetPlayersByNames(List<string> names);

    }
}
