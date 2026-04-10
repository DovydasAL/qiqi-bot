using QiQiBot.Models;

namespace QiQiBot.Services.Abstractions
{
    public interface IClanMembershipService
    {
        Task UpdateClanMembers(long clanId, List<Player> members);
        Task SetLastScraped(long clanId, DateTime date);
    }
}
