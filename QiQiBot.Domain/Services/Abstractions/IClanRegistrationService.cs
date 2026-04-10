using QiQiBot.Models;

namespace QiQiBot.Services.Abstractions
{
    public interface IClanRegistrationService
    {
        Task RegisterClan(string clanName, ulong guildId);
    }
}
