using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using QiQiBot.Models;
using System.Text.RegularExpressions;
using static QiQiBot.Models.RuneMetricsProfileDTO;

namespace QiQiBot.Services
{
    public sealed class CitadelActivityService : ICitadelActivityService
    {
        private readonly IPlayerService _playerService;
        private readonly IClanService _clanService;
        private readonly DiscordSocketClient _discordClient;
        private readonly ILogger<CitadelActivityService> _logger;

        private const int MaxNotificationsPerMessage = 10;

        private enum CitadelEventType
        {
            Visited,
            Capped
        }

        private sealed record CitadelEvent(RuneMetricsActivityDTO Activity, CitadelEventType Type);

        private static readonly Regex CitadelVisitRegex = new(@".*Visited my Clan Citadel.*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex CitadelCapRegex = new(@".*capped at my clan citadel.*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public CitadelActivityService(
            IPlayerService playerService,
            IClanService clanService,
            DiscordSocketClient discordClient,
            ILogger<CitadelActivityService> logger)
        {
            _playerService = playerService;
            _clanService = clanService;
            _discordClient = discordClient;
            _logger = logger;
        }

        public async Task ProcessCitadelActivitiesAsync(List<RuneMetricsProfileDTO> profiles, CancellationToken cancellationToken)
        {
            if (profiles == null || profiles.Count == 0)
            {
                _logger.LogInformation("No RuneMetrics profiles provided for citadel event processing.");
                return;
            }

            var citadelEvents = await GetCitadelEventsToSend(profiles, cancellationToken);

            if (citadelEvents.Count == 0)
            {
                _logger.LogInformation("No new citadel events detected.");
                return;
            }

            var clans = await _clanService.GetClans();
            await SendCitadelNotifications(clans, citadelEvents, cancellationToken);
        }

        private async Task<Dictionary<Player, List<CitadelEvent>>> GetCitadelEventsToSend(
            List<RuneMetricsProfileDTO> profiles,
            CancellationToken cancellationToken)
        {
            var filteredProfiles = profiles.Where(profile => string.IsNullOrEmpty(profile.Error)).ToList();

            if (filteredProfiles.Count == 0)
            {
                return new Dictionary<Player, List<CitadelEvent>>();
            }

            var players = await _playerService.GetPlayersByNames(filteredProfiles.Select(p => p.Name).ToList());
            var result = new Dictionary<Player, List<CitadelEvent>>();
            var profileDict = filteredProfiles.ToDictionary(x => x.Name, x => x);

            foreach (var player in players)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (player.MostRecentRuneMetricsEvent == null)
                    {
                        _logger.LogInformation(
                            "Player {Name} does not have a recorded most recent RuneMetrics event, skipping citadel events.",
                            player.Name);
                        continue;
                    }

                    if (!profileDict.TryGetValue(player.Name, out var profile))
                    {
                        _logger.LogWarning(
                            "RuneMetrics profile not found for player {Name} while processing citadel events.",
                            player.Name);
                        continue;
                    }

                    var activities = profile.Activities;
                    if (activities == null || activities.Count == 0)
                    {
                        continue;
                    }

                    var events = activities
                        .Where(x => x.RuneMetricsStringDateToObject() > player.MostRecentRuneMetricsEvent)
                        .Select(activity => CreateCitadelEvent(activity))
                        .Where(evt => evt != null)
                        .Select(evt => evt!)
                        .ToList();

                    if (events.Count > 0)
                    {
                        result[player] = events;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting citadel updates for player {Name}", player.Name);
                }
            }

            return result;
        }

        private static CitadelEvent? CreateCitadelEvent(RuneMetricsActivityDTO activity)
        {
            if (activity == null || string.IsNullOrWhiteSpace(activity.Text))
            {
                return null;
            }

            if (CitadelCapRegex.IsMatch(activity.Text))
            {
                return new CitadelEvent(activity, CitadelEventType.Capped);
            }

            if (CitadelVisitRegex.IsMatch(activity.Text))
            {
                return new CitadelEvent(activity, CitadelEventType.Visited);
            }

            return null;
        }

        private async Task SendCitadelNotifications(
            List<Clan> clans,
            Dictionary<Player, List<CitadelEvent>> playerEvents,
            CancellationToken cancellationToken)
        {
            var clanDict = clans.ToDictionary(x => x.Id, x => x);
            var groupingByClan = playerEvents.GroupBy(x => x.Key.ClanId);

            foreach (var group in groupingByClan)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var clanId = group.Key;
                if (clanId == null || !clanDict.TryGetValue(clanId.Value, out var clan))
                {
                    var firstPlayer = group.FirstOrDefault().Key;
                    _logger.LogWarning(
                        "Player {Player} has unknown clan ID {ClanId}, skipping citadel events.",
                        firstPlayer?.Name,
                        clanId);
                    continue;
                }

                var messages = new List<string>();
                foreach (var kvp in group)
                {
                    var player = kvp.Key;
                    var events = kvp.Value.OrderBy(x => x.Activity.RuneMetricsStringDateToObject());
                    foreach (var evt in events)
                    {
                        var timestamp = evt.Activity.RuneMetricsStringDateToObject();
                        var message = evt.Type switch
                        {
                            CitadelEventType.Visited => $"{timestamp:g}: {player.Name} visited the clan citadel.",
                            CitadelEventType.Capped => $"{timestamp:g}: {player.Name} capped at the clan citadel.",
                            _ => null
                        };

                        if (!string.IsNullOrEmpty(message))
                        {
                            messages.Add(message);
                        }
                    }
                }

                if (messages.Count == 0)
                {
                    continue;
                }

                foreach (var guild in clan.Guilds)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (guild.ClanCitadelChannelId == null)
                    {
                        _logger.LogWarning(
                            "Guild {GuildId} for clan {ClanName} does not have a citadel notification channel configured, skipping.",
                            guild.Id,
                            clan.Name);
                        continue;
                    }

                    var discordGuild = _discordClient.GetGuild(guild.GuildId);
                    if (discordGuild == null)
                    {
                        _logger.LogWarning(
                            "Discord guild {GuildId} not found for clan {ClanName}, skipping citadel notifications.",
                            guild.GuildId,
                            clan.Name);
                        continue;
                    }

                    var channel = discordGuild.GetTextChannel(guild.ClanCitadelChannelId.Value);
                    if (channel == null)
                    {
                        _logger.LogWarning(
                            "Citadel notification channel {ChannelId} for guild {GuildId} not found, skipping.",
                            guild.ClanCitadelChannelId.Value,
                            guild.Id);
                        continue;
                    }

                    var messageBatches = messages
                        .Chunk(MaxNotificationsPerMessage)
                        .Select(batch => string.Join(Environment.NewLine, batch));

                    foreach (var batch in messageBatches)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await channel.SendMessageAsync(batch);
                    }
                }
            }
        }
    }
}