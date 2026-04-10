namespace QiQiBot.Services.Abstractions
{
    public interface IGuildConfigurationService
    {
        Task SetAchievementChannel(ulong guildId, ulong? channelId);
        Task SetLeaveJoinChannel(ulong guildId, ulong? channelId);
        Task SetWelcomeChannel(ulong guildId, ulong? channelId);
        Task SetCitadelChannel(ulong guildId, ulong? channelId);
        Task SetCitadelResetTime(ulong guildId, long day, string time);
        Task SetDebugMode(ulong guildId, bool enabled);
    }
}
