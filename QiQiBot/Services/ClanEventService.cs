using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Text;

namespace QiQiBot.Services
{
    public class ClanEventService : IClanEventService
    {
        private readonly DiscordSocketClient _client;
        private readonly IClanService _clanService;
        private readonly ILogger<IClanEventService> _logger;
        public ClanEventService(DiscordSocketClient client, IClanService clanService, ILogger<IClanEventService> logger)
        {
            _client = client;
            _clanService = clanService;
            _logger = logger;
        }

        public async Task SendPlayerJoinEvent(ulong guildId, List<string> playerNames)
        {
            var dbGuild = await _clanService.GetGuild(guildId);
            if (dbGuild.ClanLeaveJoinChannelId == null)
            {
                _logger.LogTrace($"Guild {guildId} does not have a clan join/leave channel set, skipping player join event.");
                return;
            }
            var sb = new StringBuilder();
            foreach (var player in playerNames)
            {
                sb.AppendLine($"**{player}** has joined the clan!");
            }
            await SendNotification(sb.ToString(), guildId, dbGuild.ClanLeaveJoinChannelId.Value);
        }

        public async Task SendPlayerLeftEvent(ulong guildId, List<string> playerNames)
        {
            var dbGuild = await _clanService.GetGuild(guildId);
            if (dbGuild.ClanLeaveJoinChannelId == null)
            {
                _logger.LogTrace($"Guild {guildId} does not have a clan join/leave channel set, skipping player left event.");
                return;
            }
            var sb = new StringBuilder();
            foreach (var player in playerNames)
            {
                sb.AppendLine($"**{player}** is no longer in the clan");
            }
            await SendNotification(sb.ToString(), guildId, dbGuild.ClanLeaveJoinChannelId.Value);
        }

        private async Task SendNotification(string message, ulong guildId, ulong channelId)
        {

            var guild = _client.GetGuild(guildId);
            if (guild == null)
            {
                _logger.LogWarning($"Guild {guildId} not found in Discord client, cannot send player notification.");
                return;
            }
            var channel = guild.GetTextChannel(channelId);
            if (channel == null)
            {
                _logger.LogWarning($"Channel {channelId} not found in guild {guildId}, cannot send player notification.");
                return;
            }
            await channel.SendMessageAsync(message);
        }
    }
}
