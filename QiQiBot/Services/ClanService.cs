using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QiQiBot.Models;

namespace QiQiBot.Services
{
    public class ClanService : IClanService
    {
        private ClanContext _dbContext;
        private ILogger<ClanService> _logger;
        public ClanService(ClanContext dbContext, ILogger<ClanService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task RegisterClan(string clanName, ulong guildId)
        {
            var existing = _dbContext.Clans.FirstOrDefault(x => x.GuildId == guildId);
            if (existing != null)
            {
                _logger.LogInformation($"Updating clan for guild {guildId} from {existing.Name} to {clanName}");
                existing.Name = clanName;
                _dbContext.Clans.Update(existing);
            }
            else
            {
                _logger.LogInformation($"Creating clan for guild {guildId} with name {clanName}");
                var clan = new Clan()
                {
                    Name = clanName,
                    GuildId = guildId
                };
                await _dbContext.AddAsync(clan);
            }
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Clan> GetClanAsync(ulong guildId)
        {
            var clan = await _dbContext.Clans.FirstOrDefaultAsync(x => x.GuildId == guildId);
            if (clan == null)
            {
                throw new Exception($"No clan found for guild id: {guildId}");
            }
            return clan;
        }

        public Task<List<ClanMember>> GetClanMembers(long clanId)
        {
            return _dbContext.ClanMembers.Where(x => x.ClanId == clanId).ToListAsync();
        }

        public async Task<List<Clan>> GetClans()
        {
            return await _dbContext.Clans.ToListAsync();
        }

        public async Task UpdateClanMembers(long clanId, List<ClanMember> members)
        {
            var updateDate = DateTime.UtcNow;
            var existingMembers = await GetClanMembers(clanId);
            var existingMemberDictionary = existingMembers.ToDictionary(m => m.Name, m => m);
            var totalNew = 0;
            var totalUpdated = 0;
            foreach (var member in members)
            {
                if (existingMemberDictionary.TryGetValue(member.Name, out var existingMember))
                {
                    if (member.ClanId != existingMember.ClanId)
                    {
                        existingMember.ClanId = member.ClanId;
                        existingMember.ClanExperience = member.ClanExperience;
                        totalUpdated++;
                    }
                    else if (member.ClanExperience > existingMember.ClanExperience)
                    {
                        existingMember.ClanExperience = member.ClanExperience;
                        existingMember.LastClanExperienceUpdate = updateDate;
                        totalUpdated++;
                    }

                }
                else
                {
                    _dbContext.ClanMembers.Add(member);
                    totalNew++;
                }
            }
            _logger.LogInformation($"Updating Clan {clanId} - Total members: {members.Count}, New members: {totalNew}, Updated members: {totalUpdated}");
            await _dbContext.SaveChangesAsync();
        }

        public async Task SetLastScraped(long clanId, DateTime date)
        {
            var clan = await _dbContext.Clans.FirstOrDefaultAsync(x => x.Id == clanId);
            if (clan != null)
            {
                _logger.LogInformation($"Setting last scraped for clan {clanId} to {date}");
                clan.LastScraped = date;
                await _dbContext.SaveChangesAsync();
            }
        }

    }
}
