using Microsoft.EntityFrameworkCore;
using QiQiBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QiQiBot.Services
{
    public class ClanService : IClanService
    {
        private ClanContext _dbContext;
        public ClanService(ClanContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task RegisterClan(string clanName, ulong guildId)
        {
            var existing = _dbContext.Clans.FirstOrDefault(x => x.GuildId == guildId);
            if (existing != null)
            {
                existing.Name = clanName;
                _dbContext.Clans.Update(existing);
            }
            else
            {
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
            var existingMembers = await GetClanMembers(clanId);
            var existingMemberDictionary = existingMembers.ToDictionary(m => m.Name, m => m);

            foreach (var member in members)
            {
                if (existingMemberDictionary.TryGetValue(member.Name, out var existingMember))
                {
                    if (member.Experience > existingMember.Experience)
                    {
                        existingMember.Experience = member.Experience;
                        existingMember.LastExperienceUpdate = DateTime.UtcNow;
                    }
                    if (member.ClanId != existingMember.ClanId)
                    {
                        existingMember.ClanId = member.ClanId;
                    }
                }
                else
                {
                    member.LastExperienceUpdate = DateTime.UtcNow;
                    _dbContext.ClanMembers.Add(member);
                }
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
