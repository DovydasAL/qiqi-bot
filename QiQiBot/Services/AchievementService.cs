using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using QiQiBot.Models;
using System.Text;
using System.Text.RegularExpressions;
using static QiQiBot.Models.RuneMetricsProfileDTO;

namespace QiQiBot.Services
{
    public sealed class AchievementService : IAchievementService
    {
        private readonly IPlayerService _playerService;
        private readonly IClanService _clanService;
        private readonly DiscordSocketClient _discordClient;
        private readonly ILogger<AchievementService> _logger;

        private static readonly string[] FilterActivityTextRegexStrings = new[]
        {
            @".*(?!200000000(?:\D|$))\d+XP.*",
            @".*songs unlocked.*",
            @".*Quest complete.*",
            @".*Clan Fealty.*",
            @".*Visited my Clan Citadel.*",
            @".*capped at my clan citadel.*",
            @".*crystal triskelion fragment.*",
            @".*abyssal whip.*",
            @".*dragon helm.*",
            @".*shield left half.*",
            @".*dragon boots.*",
            @".*dragon hatchet.*",
            @".*archaeological mystery.*",
            @".*songs unlocked.*",
            @".*killed.*",
            @".*defeated.*",
        };

        private static readonly string[] FilterActivityDetailRegexStrings = new[]
        {
            @".*am now level (?!99\b|110\b|120\b)\d+.*",
        };

        private static readonly Regex[] FilterActivityTextRegexes = FilterActivityTextRegexStrings
            .Select(pattern => new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
            .ToArray();

        private static readonly Regex[] FilterActivityDetailRegexes = FilterActivityDetailRegexStrings
            .Select(pattern => new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase))
            .ToArray();

        private const int MaxAchievementsPerMessage = 10;

        public AchievementService(
            IPlayerService playerService,
            IClanService clanService,
            DiscordSocketClient discordClient,
            ILogger<AchievementService> logger)
        {
            _playerService = playerService;
            _clanService = clanService;
            _discordClient = discordClient;
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

                var activityMessages = new List<string>();
                foreach (var kvp in group)
                {
                    var player = kvp.Key;
                    var activities = kvp.Value.OrderBy(x => x.RuneMetricsStringDateToObject());
                    foreach (var activity in activities)
                    {
                        var prefix = ShouldFilterActivity(activity) ? "[To Be Filtered] " : string.Empty;
                        var message = $"{prefix}{activity.RuneMetricsStringDateToObject():g}: {player.Name}: {activity.Details}";
                        activityMessages.Add(message);
                    }
                }

                if (activityMessages.Count == 0)
                {
                    continue;
                }

                var messageBatches = activityMessages
                    .Chunk(MaxAchievementsPerMessage)
                    .Select(batch => string.Join(Environment.NewLine, batch))
                    .ToList();

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

                    var discordGuild = _discordClient.GetGuild(guild.GuildId);
                    if (discordGuild == null)
                    {
                        _logger.LogWarning(
                            "Discord guild {GuildId} not found for clan {ClanName}, skipping.",
                            guild.GuildId,
                            clan.Name);
                        continue;
                    }

                    var channel = discordGuild.GetTextChannel(guild.AchievementsChannelId.Value);
                    if (channel == null)
                    {
                        _logger.LogWarning(
                            "Achievements channel {ChannelId} for guild {GuildId} not found, skipping.",
                            guild.AchievementsChannelId.Value,
                            guild.Id);
                        continue;
                    }

                    foreach (var batch in messageBatches)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await channel.SendMessageAsync(batch);
                    }
                }
            }
        }

        private static bool ShouldFilterActivity(RuneMetricsActivityDTO activity)
        {
            if (string.IsNullOrEmpty(activity.Text))
            {
                return false;
            }

            return FilterActivityTextRegexes.Any(regex => regex.IsMatch(activity.Text)) || FilterActivityDetailRegexes.Any(regex => regex.IsMatch(activity.Details));
        }
    }
}
