using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QiQiBot.Models;
using QiQiBot.Services.Abstractions;

namespace QiQiBot.Services
{
    public class ClanMembershipService : IClanMembershipService
    {
        private readonly ClanContext _dbContext;
        private readonly ILogger<ClanMembershipService> _logger;

        public ClanMembershipService(ClanContext dbContext, ILogger<ClanMembershipService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task UpdateClanMembers(long clanId, List<Player> members)
        {
            var updateDate = DateTime.UtcNow;
            var existingMembers = await _dbContext.Players.Where(x => x.ClanId == clanId).ToListAsync();
            var existingMemberDictionary = existingMembers.ToDictionary(m => m.Name, m => m);
            var knownMembersByName = await _dbContext.Players.Where(p => members.Select(x => x.Name).Contains(p.Name)).ToDictionaryAsync(p => p.Name, p => p);
            var totalNew = 0;
            var totalUpdated = 0;
            var totalDeleted = 0;
            foreach (var member in members)
            {
                if (existingMemberDictionary.TryGetValue(member.Name, out var existingMember))
                {
                    if (member.ClanExperience > existingMember.ClanExperience)
                    {
                        existingMember.ClanExperience = member.ClanExperience;
                        existingMember.LastClanExperienceUpdate = updateDate;
                        totalUpdated++;
                    }
                }
                else if (knownMembersByName.TryGetValue(member.Name, out var knownMember))
                {
                    knownMember.ClanId = clanId;
                    knownMember.ClanExperience = member.ClanExperience;
                    knownMember.LastClanExperienceUpdate = updateDate;
                    totalNew++;
                }
                else
                {
                    _dbContext.Players.Add(member);
                    totalNew++;
                }
            }
            var membersDictionary = members.ToDictionary(m => m.Name, m => m);
            foreach (var existingMember in existingMembers)
            {
                if (!membersDictionary.ContainsKey(existingMember.Name))
                {
                    existingMember.ClanId = null;
                    totalDeleted++;
                }
            }
            _logger.LogInformation($"Updating Clan {clanId} - Total members: {members.Count}, New members: {totalNew}, Updated members: {totalUpdated}, Deleted members: {totalDeleted}");
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
