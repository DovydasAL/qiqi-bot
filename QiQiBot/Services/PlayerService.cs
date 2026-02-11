using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QiQiBot.Models;

namespace QiQiBot.Services
{
    public class PlayerService : IPlayerService
    {

        private ClanContext _dbContext;
        private ILogger<PlayerService> _logger;
        public PlayerService(ClanContext dbContext, ILogger<PlayerService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task UpdatePlayersFromRuneMetrics(List<string> names, List<RuneMetricsProfileDTO> profiles)
        {
            var operationDate = DateTime.UtcNow;
            var existingMembers = await _dbContext.ClanMembers.Where(x => names.Contains(x.Name)).ToListAsync();
            var profilesDictionary = profiles.ToDictionary(p => p.Name, p => p);
            foreach (var member in existingMembers)
            {
                try
                {
                    if (profilesDictionary.TryGetValue(member.Name, out var profile))
                    {
                        member.LastScrapedRuneMetricsProfile = operationDate;
                        if (!string.IsNullOrEmpty(profile.Error))
                        {
                            if (profile.Error == "PROFILE_PRIVATE")
                            {
                                member.PrivateRuneMetricsProfile = true;
                            }
                            if (profile.Error == "NO_PROFILE")
                            {
                                member.InvalidRuneMetricsProfile = true;
                            }
                            continue;
                        }
                        var mostRecentActivityDateString = profilesDictionary[member.Name].Activities.OrderByDescending(x => x.Date).First().Date;
                        var mostRecentActivityDate = DateTime.SpecifyKind(DateTime.Parse(mostRecentActivityDateString), DateTimeKind.Utc);
                        member.MostRecentRuneMetricsEvent = mostRecentActivityDate;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating player {PlayerName} from RuneMetrics", member.Name);
                }
            }
            await _dbContext.SaveChangesAsync();
        }

        public Task<List<ClanMember>> GetLeastRecentlyScrapedMembers(int n, TimeSpan olderThan)
        {
            return _dbContext.ClanMembers
                .Where(x => !x.PrivateRuneMetricsProfile && !x.InvalidRuneMetricsProfile && (x.LastScrapedRuneMetricsProfile == null || x.LastScrapedRuneMetricsProfile < DateTime.UtcNow - olderThan))
                .OrderBy(x => x.LastScrapedRuneMetricsProfile == null).ThenBy(x => x.LastScrapedRuneMetricsProfile)
                .Take(n).ToListAsync();
        }
    }
}
