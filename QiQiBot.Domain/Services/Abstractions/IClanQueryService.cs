using QiQiBot.Models;

namespace QiQiBot.Services.Abstractions
{
    public interface IClanQueryService
    {
        Task<Clan> GetClanAsync(ulong guildId);
        Task<List<Clan>> GetClans();
        Task<Guild> GetGuild(ulong guildId);
        Task<List<Player>> GetClanMembers(long clanId);
    }
}
