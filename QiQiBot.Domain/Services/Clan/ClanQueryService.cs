using Microsoft.EntityFrameworkCore;
using QiQiBot.Exceptions;
using QiQiBot.Models;
using QiQiBot.Services.Abstractions;

namespace QiQiBot.Services
{
    public class ClanQueryService : IClanQueryService
    {
        private readonly ClanContext _dbContext;

        public ClanQueryService(ClanContext dbContext)
        {
            _dbContext = dbContext;
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

        public async Task<Guild> GetGuild(ulong guildId)
        {
            var guild = await _dbContext.Guilds.Include(x => x.Clan).FirstOrDefaultAsync(x => x.GuildId == guildId);
            if (guild == null)
            {
                throw new NoClanRegisteredException(guildId);
            }

            return guild;
        }
    }
}
