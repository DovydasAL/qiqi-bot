using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QiQiBot.Models;
using QiQiBot.Services.Abstractions;

namespace QiQiBot.Services
{
    public class ClanRegistrationService : IClanRegistrationService
    {
        private readonly ClanContext _dbContext;
        private readonly ILogger<ClanRegistrationService> _logger;

        public ClanRegistrationService(ClanContext dbContext, ILogger<ClanRegistrationService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task RegisterClan(string clanName, ulong guildId)
        {
            var guild = await _dbContext.Guilds.Include(x => x.Clan).FirstOrDefaultAsync(x => x.GuildId == guildId);
            if (guild == null)
            {
                guild = new Guild
                {
                    GuildId = guildId
                };
                await _dbContext.Guilds.AddAsync(guild);
            }

            var clan = await _dbContext.Clans.FirstOrDefaultAsync(x => x.Name == clanName);
            if (clan == null)
            {
                _logger.LogInformation($"Creating clan {clanName} for guild {guildId}");
                clan = new Clan
                {
                    Name = clanName
                };
                await _dbContext.Clans.AddAsync(clan);
            }

            var previousClanName = guild.Clan?.Name;
            var isSameClan = guild.ClanId.HasValue && clan.Id != 0 && guild.ClanId.Value == clan.Id;
            if (isSameClan)
            {
                _logger.LogInformation($"Guild {guildId} already registered to clan {clanName}");
            }
            else
            {
                if (previousClanName == null)
                {
                    _logger.LogInformation($"Assigning clan {clanName} to guild {guildId}");
                }
                else
                {
                    _logger.LogInformation($"Updating clan for guild {guildId} from {previousClanName} to {clanName}");
                }

                guild.Clan = clan;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
