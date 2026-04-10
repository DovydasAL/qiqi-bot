using QiQiBot.Models;
using QiQiBot.Services.Abstractions;

namespace QiQiBot.Services
{
    public class ClanService : IClanService
    {
        private readonly IClanRegistrationService _clanRegistrationService;
        private readonly IGuildConfigurationService _guildConfigurationService;
        private readonly IClanQueryService _clanQueryService;
        private readonly IClanMembershipService _clanMembershipService;

        public ClanService(
            IClanRegistrationService clanRegistrationService,
            IGuildConfigurationService guildConfigurationService,
            IClanQueryService clanQueryService,
            IClanMembershipService clanMembershipService)
        {
            _clanRegistrationService = clanRegistrationService;
            _guildConfigurationService = guildConfigurationService;
            _clanQueryService = clanQueryService;
            _clanMembershipService = clanMembershipService;
        }

        public Task RegisterClan(string clanName, ulong guildId)
            => _clanRegistrationService.RegisterClan(clanName, guildId);

        public Task SetAchievementChannel(ulong guildId, ulong? channelId)
            => _guildConfigurationService.SetAchievementChannel(guildId, channelId);

        public Task SetLeaveJoinChannel(ulong guildId, ulong? channelId)
            => _guildConfigurationService.SetLeaveJoinChannel(guildId, channelId);

        public Task SetWelcomeChannel(ulong guildId, ulong? channelId)
            => _guildConfigurationService.SetWelcomeChannel(guildId, channelId);

        public Task SetCitadelChannel(ulong guildId, ulong? channelId)
            => _guildConfigurationService.SetCitadelChannel(guildId, channelId);

        public Task<List<Clan>> GetClans()
            => _clanQueryService.GetClans();

        public Task<Clan> GetClanAsync(ulong guildId)
            => _clanQueryService.GetClanAsync(guildId);

        public Task<List<Player>> GetClanMembers(long clanId)
            => _clanQueryService.GetClanMembers(clanId);

        public Task UpdateClanMembers(long clanId, List<Player> members)
            => _clanMembershipService.UpdateClanMembers(clanId, members);

        public Task SetLastScraped(long clanId, DateTime date)
            => _clanMembershipService.SetLastScraped(clanId, date);

        public Task SetCitadelResetTime(ulong guildId, long day, string time)
            => _guildConfigurationService.SetCitadelResetTime(guildId, day, time);

        public Task SetDebugMode(ulong guildId, bool enabled)
            => _guildConfigurationService.SetDebugMode(guildId, enabled);

        public Task<Guild> GetGuild(ulong guildId)
            => _clanQueryService.GetGuild(guildId);
    }
}
