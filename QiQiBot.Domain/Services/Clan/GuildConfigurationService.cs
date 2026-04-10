using Microsoft.EntityFrameworkCore;
using QiQiBot.Exceptions;
using QiQiBot.Models;
using QiQiBot.Services.Abstractions;

namespace QiQiBot.Services
{
    public class GuildConfigurationService : IGuildConfigurationService
    {
        private readonly ClanContext _dbContext;

        public GuildConfigurationService(ClanContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SetAchievementChannel(ulong guildId, ulong? channelId)
        {
            var guild = await GetGuildOrThrow(guildId);
            guild.AchievementsChannelId = channelId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetLeaveJoinChannel(ulong guildId, ulong? channelId)
        {
            var guild = await GetGuildOrThrow(guildId);
            guild.ClanLeaveJoinChannelId = channelId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetWelcomeChannel(ulong guildId, ulong? channelId)
        {
            var guild = await GetGuildOrThrow(guildId);
            guild.ClanWelcomeChannelId = channelId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetCitadelChannel(ulong guildId, ulong? channelId)
        {
            var guild = await GetGuildOrThrow(guildId);
            guild.ClanCitadelChannelId = channelId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetCitadelResetTime(ulong guildId, long day, string time)
        {
            var guild = await GetGuildOrThrow(guildId);

            if (!TimeSpan.TryParse(time, out var capResetTime) || capResetTime < TimeSpan.Zero || capResetTime >= TimeSpan.FromDays(1))
            {
                throw new InvalidResetTimeException(time);
            }

            guild.CapResetDay = day;
            guild.CapResetTime = time;
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetDebugMode(ulong guildId, bool enabled)
        {
            var guild = await GetGuildOrThrow(guildId);
            guild.DebugModeEnabled = enabled;
            await _dbContext.SaveChangesAsync();
        }

        private async Task<Guild> GetGuildOrThrow(ulong guildId)
        {
            var guild = await _dbContext.Guilds.FirstOrDefaultAsync(x => x.GuildId == guildId);
            if (guild == null)
            {
                throw new GuildNotFoundException(guildId);
            }

            return guild;
        }
    }
}
