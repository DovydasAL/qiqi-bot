using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QiQiBot.Models;
using System.Globalization;

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
            var existingMembers = await _dbContext.Players.Where(x => names.Contains(x.Name)).ToListAsync();
            var profilesDictionary = profiles.ToDictionary(p => p.Name, p => p);
            foreach (var member in existingMembers)
            {
                try
                {
                    if (profilesDictionary.TryGetValue(member.Name, out var profile))
                    {
                        member.LastScrapedRuneMetricsProfile = profile.ScrapedDate;
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
                        var dateList = profile.Activities
                            .Select(x => RuneMetricsStringDateToObject(x.Date))
                            .ToList();
                        var mostRecentActivityDate = dateList.OrderByDescending(x => x).First();
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

        private DateTime RuneMetricsStringDateToObject(string date)
        {
            // Example: "18-Jan-2026 23:52"
            const string format = "dd-MMM-yyyy HH:mm";

            return DateTime.ParseExact(
                date,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal
            );
        }

        public Task<List<Player>> GetLeastRecentlyScrapedMembers(int n, TimeSpan olderThan)
        {
            return _dbContext.Players
                .Where(x => x.ClanId != null && !x.PrivateRuneMetricsProfile && !x.InvalidRuneMetricsProfile && (x.LastScrapedRuneMetricsProfile == null || x.LastScrapedRuneMetricsProfile < DateTime.UtcNow - olderThan))
                .OrderBy(x => x.LastScrapedRuneMetricsProfile == null).ThenBy(x => x.LastScrapedRuneMetricsProfile)
                .Take(n).ToListAsync();
        }
    }
}
