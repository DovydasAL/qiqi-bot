using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QiQiBot.Exceptions;
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

        public async Task<Clan> GetClanAsync(ulong guildId)
        {
            var guild = await _dbContext.Guilds.Include(x => x.Clan).FirstOrDefaultAsync(x => x.GuildId == guildId);
            if (guild == null || guild.Clan == null)
            {
                throw new NoClanRegisteredException(guildId);
            }
            return guild.Clan;
        }

        public Task<List<Player>> GetClanMembers(long clanId)
        {
            return _dbContext.Players.Where(x => x.ClanId == clanId).ToListAsync();
        }

        public async Task<List<Clan>> GetClans()
        {
            return await _dbContext.Clans.Include(x => x.Guilds).ToListAsync();
        }

        public async Task UpdateClanMembers(long clanId, List<Player> members)
        {
            var updateDate = DateTime.UtcNow;
            var existingMembers = await GetClanMembers(clanId);
            var existingMemberDictionary = existingMembers.ToDictionary(m => m.Name, m => m);
            var totalNew = 0;
            var totalUpdated = 0;
            var totalDeleted = 0;
            foreach (var member in members)
            {
                if (existingMemberDictionary.TryGetValue(member.Name, out var existingMember))
                {
                    // If a player switches clans, reset their experience to what we found
                    if (member.ClanId != existingMember.ClanId)
                    {
                        existingMember.ClanId = member.ClanId;
                        existingMember.ClanExperience = member.ClanExperience;
                        totalNew++;
                    }
                    // If the player is in the same clan but has more experience, update it
                    else if (member.ClanExperience > existingMember.ClanExperience)
                    {
                        existingMember.ClanExperience = member.ClanExperience;
                        existingMember.LastClanExperienceUpdate = updateDate;
                        totalUpdated++;
                    }
                }
                else
                {
                    _dbContext.Players.Add(member);
                    totalNew++;
                }
            }
            var membersDictionary = members.ToDictionary(m => m.Name, m => m);
            // If an existing member is no longer in the clan, set their ClanId to null
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
