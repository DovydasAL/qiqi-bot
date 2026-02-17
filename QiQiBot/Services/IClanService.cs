using QiQiBot.Models;

namespace QiQiBot.Services
{
    public interface IClanService
    {
        Task RegisterClan(string clanName, ulong guildId);
        Task SetAchievementChannel(ulong guildId, ulong? channelId);
        Task<List<Clan>> GetClans();
        Task<Clan> GetClanAsync(ulong guildId);
        Task<List<Player>> GetClanMembers(long clanId);
        Task UpdateClanMembers(long clanId, List<Player> members);
        Task SetLastScraped(long clanId, DateTime date);
    }
}
