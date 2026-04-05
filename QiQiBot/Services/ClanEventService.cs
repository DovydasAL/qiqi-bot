using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace QiQiBot.Services
{
    public class ClanEventService : IClanEventService
    {
        private readonly DiscordSocketClient _client;
        private readonly IClanService _clanService;
        private readonly ILogger<IClanEventService> _logger;
        private const int MaxLinesPerNotification = 20;
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
            var notificationBatches = playerNames
                .Select(player => $"**{player}** has joined the clan!")
                .Chunk(MaxLinesPerNotification)
                .Select(batch => string.Join(Environment.NewLine, batch));
            foreach (var batch in notificationBatches)
            {
                await SendNotification(batch, guildId, dbGuild.ClanLeaveJoinChannelId.Value);
            }
        }

        public async Task SendPlayerLeftEvent(ulong guildId, List<string> playerNames)
        {
            var dbGuild = await _clanService.GetGuild(guildId);
            if (dbGuild.ClanLeaveJoinChannelId == null)
            {
                _logger.LogTrace($"Guild {guildId} does not have a clan join/leave channel set, skipping player left event.");
                return;
            }
            var notificationBatches = playerNames
                .Select(player => $"**{player}** is no longer in the clan")
                .Chunk(MaxLinesPerNotification)
                .Select(batch => string.Join(Environment.NewLine, batch));
            foreach (var batch in notificationBatches)
            {
                await SendNotification(batch, guildId, dbGuild.ClanLeaveJoinChannelId.Value);
            }
        }

        public async Task SendPlayerRenameEvent(ulong guildId, List<(string OldName, string NewName)> renames)
        {
            var dbGuild = await _clanService.GetGuild(guildId);
            if (dbGuild.ClanLeaveJoinChannelId == null)
            {
                _logger.LogTrace($"Guild {guildId} does not have a clan join/leave channel set, skipping player rename event.");
                return;
            }

            var notificationBatches = renames
                .Select(rename => $"**{rename.OldName}** has renamed to **{rename.NewName}**")
                .Chunk(MaxLinesPerNotification)
                .Select(batch => string.Join(Environment.NewLine, batch));

            foreach (var batch in notificationBatches)
            {
                await SendNotification(batch, guildId, dbGuild.ClanLeaveJoinChannelId.Value);
            }
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
