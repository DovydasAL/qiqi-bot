using QiQiBot.Models;

namespace QiQiBot.Services
{
    public interface IPlayerService
    {
        Task UpdatePlayersFromRuneMetrics(List<string> names, List<RuneMetricsProfileDTO> profiles);
        Task<List<ClanMember>> GetLeastRecentlyScrapedMembers(int n, TimeSpan olderThan);

    }
}
