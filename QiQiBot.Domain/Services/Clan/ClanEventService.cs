using Microsoft.Extensions.Logging;
using QiQiBot.Services.Notifications;

namespace QiQiBot.Services
{
    public class ClanEventService : IClanEventService
    {
        private readonly IClanService _clanService;
        private readonly INotificationChannelResolver _notificationChannelResolver;
        private readonly IMessageBatcher _messageBatcher;
        private readonly IDiscordMessageSender _discordMessageSender;
        private readonly ILogger<IClanEventService> _logger;
        private const int MaxLinesPerNotification = 20;

        public ClanEventService(
            IClanService clanService,
            INotificationChannelResolver notificationChannelResolver,
            IMessageBatcher messageBatcher,
            IDiscordMessageSender discordMessageSender,
            ILogger<IClanEventService> logger)
        {
            _clanService = clanService;
            _notificationChannelResolver = notificationChannelResolver;
            _messageBatcher = messageBatcher;
            _discordMessageSender = discordMessageSender;
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

            var channel = _notificationChannelResolver.ResolveTextChannel(
                guildId,
                dbGuild.ClanLeaveJoinChannelId.Value,
                "join/leave");

            if (channel == null)
            {
                return;
            }

            var notificationBatches = _messageBatcher.BatchLines(
                playerNames.Select(player => $"**{player}** has joined the clan!"),
                MaxLinesPerNotification);

            await _discordMessageSender.SendBatchesAsync(channel, notificationBatches, CancellationToken.None);
        }

        public async Task SendPlayerLeftEvent(ulong guildId, List<string> playerNames)
        {
            var dbGuild = await _clanService.GetGuild(guildId);
            if (dbGuild.ClanLeaveJoinChannelId == null)
            {
                _logger.LogTrace($"Guild {guildId} does not have a clan join/leave channel set, skipping player left event.");
                return;
            }

            var channel = _notificationChannelResolver.ResolveTextChannel(
                guildId,
                dbGuild.ClanLeaveJoinChannelId.Value,
                "join/leave");

            if (channel == null)
            {
                return;
            }

            var notificationBatches = _messageBatcher.BatchLines(
                playerNames.Select(player => $"**{player}** is no longer in the clan"),
                MaxLinesPerNotification);

            await _discordMessageSender.SendBatchesAsync(channel, notificationBatches, CancellationToken.None);
        }

        public async Task SendPlayerRenameEvent(ulong guildId, List<(string OldName, string NewName)> renames)
        {
            var dbGuild = await _clanService.GetGuild(guildId);
            if (dbGuild.ClanLeaveJoinChannelId == null)
            {
                _logger.LogTrace($"Guild {guildId} does not have a clan join/leave channel set, skipping player rename event.");
                return;
            }

            var channel = _notificationChannelResolver.ResolveTextChannel(
                guildId,
                dbGuild.ClanLeaveJoinChannelId.Value,
                "join/leave");

            if (channel == null)
            {
                return;
            }

            var notificationBatches = _messageBatcher.BatchLines(
                renames.Select(rename => $"**{rename.OldName}** has renamed to **{rename.NewName}**"),
                MaxLinesPerNotification);

            await _discordMessageSender.SendBatchesAsync(channel, notificationBatches, CancellationToken.None);
        }
    }
}
