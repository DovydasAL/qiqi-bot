namespace QiQiBot.Services;

public interface IRsnService
{
    Task SetRsnAsync(ulong guildId, ulong userId, string runescapeName);
    Task<string?> GetRsnAsync(ulong guildId, ulong userId);
}
