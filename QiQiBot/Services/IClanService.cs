using QiQiBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QiQiBot.Services
{
    public interface IClanService
    {
        Task RegisterClan(string clanName, ulong guildId);
        Task<List<Clan>> GetClans();
        Task<Clan> GetClanAsync(ulong guildId);
        Task<List<ClanMember>> GetClanMembers(long clanId);
        Task UpdateClanMembers(long clanId, List<ClanMember> members);
    }
}
