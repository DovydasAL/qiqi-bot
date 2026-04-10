using Microsoft.EntityFrameworkCore;
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
            var guild = await _dbContext.Guilds.FirstOrDefaultAsync(x => x.GuildId == guildId);
            if (guild == null)
            {
                throw new Exception($"No guild found with guildId {guildId}");
            }

            guild.AchievementsChannelId = channelId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetLeaveJoinChannel(ulong guildId, ulong? channelId)
        {
            var guild = await _dbContext.Guilds.FirstOrDefaultAsync(x => x.GuildId == guildId);
            if (guild == null)
            {
                throw new Exception($"No guild found with guildId {guildId}");
            }

            guild.ClanLeaveJoinChannelId = channelId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetWelcomeChannel(ulong guildId, ulong? channelId)
        {
            var guild = await _dbContext.Guilds.FirstOrDefaultAsync(x => x.GuildId == guildId);
            if (guild == null)
            {
                throw new Exception($"No guild found with guildId {guildId}");
            }

            guild.ClanWelcomeChannelId = channelId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetCitadelChannel(ulong guildId, ulong? channelId)
        {
            var guild = await _dbContext.Guilds.FirstOrDefaultAsync(x => x.GuildId == guildId);
            if (guild == null)
            {
                throw new Exception($"No guild found with guildId {guildId}");
            }

            guild.ClanCitadelChannelId = channelId;
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetCitadelResetTime(ulong guildId, long day, string time)
        {
            var guild = await _dbContext.Guilds.FirstOrDefaultAsync(x => x.GuildId == guildId);
            if (guild == null)
            {
                throw new Exception($"No guild found with guildId {guildId}");
            }

            // Ensure time is in the format HH:mm and between 00:00 and 23:59
            if (!TimeSpan.TryParse(time, out var capResetTime) || capResetTime < TimeSpan.Zero || capResetTime >= TimeSpan.FromDays(1))
            {
                throw new Exception($"Invalid cap reset time: {time}. Time must be in the format HH:mm and between 00:00 and 23:59.");
            }

            guild.CapResetDay = day;
            guild.CapResetTime = time;
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetDebugMode(ulong guildId, bool enabled)
        {
            var guild = await _dbContext.Guilds.FirstOrDefaultAsync(x => x.GuildId == guildId);
            if (guild == null)
            {
                throw new Exception($"No guild found with guildId {guildId}");
            }

            guild.DebugModeEnabled = enabled;
            await _dbContext.SaveChangesAsync();
        }
    }
}
