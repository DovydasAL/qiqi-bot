using Microsoft.Extensions.Logging;
using QiQiBot.Models;
using QiQiBot.Services.Notifications;
using QiQiBot.Services.RuneMetrics;
using static QiQiBot.Models.RuneMetricsProfileDTO;

namespace QiQiBot.Services
{
    public sealed class AchievementService : IAchievementService
    {
        private readonly IPlayerService _playerService;
        private readonly IClanService _clanService;
        private readonly INotificationChannelResolver _notificationChannelResolver;
        private readonly IMessageBatcher _messageBatcher;
        private readonly IDiscordMessageSender _discordMessageSender;
        private readonly IAchievementFilter _achievementFilter;
        private readonly ILogger<AchievementService> _logger;

        private const int MaxAchievementsPerMessage = 10;
        private sealed record AchievementMessage(string Text, bool IsFiltered);

        public AchievementService(
            IPlayerService playerService,
            IClanService clanService,
            INotificationChannelResolver notificationChannelResolver,
            IMessageBatcher messageBatcher,
            IDiscordMessageSender discordMessageSender,
            IAchievementFilter achievementFilter,
            ILogger<AchievementService> logger)
        {
            _playerService = playerService;
            _clanService = clanService;
            _notificationChannelResolver = notificationChannelResolver;
            _messageBatcher = messageBatcher;
            _discordMessageSender = discordMessageSender;
            _achievementFilter = achievementFilter;
            _logger = logger;
        }

        public async Task ProcessAchievementsAsync(List<RuneMetricsProfileDTO> profiles, CancellationToken cancellationToken)
        {
            if (profiles == null || profiles.Count == 0)
            {
                _logger.LogInformation("No RuneMetrics profiles provided for achievement processing.");
                return;
            }

            var achievements = await GetAchievementsToSend(profiles, cancellationToken);

            if (achievements.Count == 0)
            {
                _logger.LogInformation("No new achievements detected.");
                return;
            }

            var clans = await _clanService.GetClans();
            await SendAchievements(clans, achievements, cancellationToken);
        }

        private async Task<Dictionary<Player, List<RuneMetricsActivityDTO>>> GetAchievementsToSend(
            List<RuneMetricsProfileDTO> profiles,
            CancellationToken cancellationToken)
        {
            var filteredProfiles = profiles.Where(profile => string.IsNullOrEmpty(profile.Error)).ToList();

            if (filteredProfiles.Count == 0)
            {
                return new Dictionary<Player, List<RuneMetricsActivityDTO>>();
            }

            var players = await _playerService.GetPlayersByNames(filteredProfiles.Select(p => p.Name).ToList());
            var result = new Dictionary<Player, List<RuneMetricsActivityDTO>>();
            var profileDict = filteredProfiles.ToDictionary(x => x.Name, x => x);

            foreach (var player in players)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (player.MostRecentRuneMetricsEvent == null)
                    {
                        _logger.LogInformation(
                            "Player {Name} does not have a recorded most recent RuneMetrics event, skipping achievement check.",
                            player.Name);
                        continue;
                    }

                    if (!profileDict.TryGetValue(player.Name, out var profile))
                    {
                        _logger.LogWarning("RuneMetrics profile not found for player {Name} while processing achievements.", player.Name);
                        continue;
                    }

                    var newActivities = profile.Activities
                        .Where(x => x.RuneMetricsStringDateToObject() > player.MostRecentRuneMetricsEvent)
                        .ToList();

                    if (newActivities.Count > 0)
                    {
                        result[player] = newActivities;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting achievement updates for player {Name}", player.Name);
                }
            }

            return result;
        }

        private async Task SendAchievements(
            List<Clan> clans,
            Dictionary<Player, List<RuneMetricsActivityDTO>> achievements,
            CancellationToken cancellationToken)
        {
            var clanDict = clans.ToDictionary(x => x.Id, x => x);
            var groupingByClan = achievements.GroupBy(x => x.Key.ClanId);

            foreach (var group in groupingByClan)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var clanId = group.Key;
                if (clanId == null || !clanDict.TryGetValue(clanId.Value, out var clan))
                {
                    var firstPlayer = group.FirstOrDefault().Key;
                    _logger.LogWarning("Player {Player} has unknown clan ID {ClanId}, skipping achievements.", firstPlayer?.Name, clanId);
                    continue;
                }

                var activityMessages = new List<AchievementMessage>();
                foreach (var kvp in group)
                {
                    var player = kvp.Key;
                    var activities = kvp.Value.OrderBy(x => x.RuneMetricsStringDateToObject());
                    foreach (var activity in activities)
                    {
                        var isFiltered = _achievementFilter.IsFiltered(activity);
                        var secondsSinceEpoch = activity.RuneMetricsStringDateToObject().Subtract(DateTime.UnixEpoch).TotalSeconds;
                        var message = $"[<t:{secondsSinceEpoch}:f>] **{player.Name}**: {activity.Details}";
                        activityMessages.Add(new AchievementMessage(message, isFiltered));
                    }
                }

                if (activityMessages.Count == 0)
                {
                    continue;
                }

                foreach (var guild in clan.Guilds)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (guild.AchievementsChannelId == null)
                    {
                        _logger.LogWarning(
                            "Guild {GuildId} for clan {ClanName} does not have an achievements channel configured, skipping.",
                            guild.Id,
                            clan.Name);
                        continue;
                    }

                    var channel = _notificationChannelResolver.ResolveTextChannel(
                        guild.GuildId,
                        guild.AchievementsChannelId.Value,
                        "achievement");

                    if (channel == null)
                    {
                        continue;
                    }

                    var guildMessages = new List<string>();
                    foreach (var achievement in activityMessages)
                    {
                        if (achievement.IsFiltered)
                        {
                            if (!guild.DebugModeEnabled)
                            {
                                continue;
                            }

                            guildMessages.Add($"[Filtered] {achievement.Text}");
                        }
                        else
                        {
                            guildMessages.Add(achievement.Text);
                        }
                    }

                    if (guildMessages.Count == 0)
                    {
                        continue;
                    }

                    var messageBatches = _messageBatcher.BatchLines(guildMessages, MaxAchievementsPerMessage);
                    await _discordMessageSender.SendBatchesAsync(channel, messageBatches, cancellationToken);
                }
            }
        }
    }
}
