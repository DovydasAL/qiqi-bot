using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QiQiBot.Exceptions;
using QiQiBot.Models;
using QiQiBot.Services;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static QiQiBot.Models.RuneMetricsProfileDTO;

namespace QiQiBot.HostedServices;

public sealed class PlayerScrapeService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<PlayerScrapeService> _logger;

    // how often to check for players to scrape
    private static readonly TimeSpan GlobalInterval = TimeSpan.FromMinutes(5);
    // minimum age since last scrape
    private static readonly TimeSpan MinProfileAge = TimeSpan.FromMinutes(120);
    // delay between individual player requests
    private static readonly TimeSpan PerPlayerDelay = TimeSpan.FromSeconds(1);
    // backoff when receiving 429
    private static readonly TimeSpan TooManyRequestsDelay = TimeSpan.FromSeconds(10);

    // batch size
    private const int BatchSize = 100;

    public PlayerScrapeService(
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<PlayerScrapeService> logger)
    {
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PlayerScrapeService started.");

        using var timer = new PeriodicTimer(GlobalInterval);

        do
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var playerService = scope.ServiceProvider.GetRequiredService<IPlayerService>();

                _logger.LogInformation("Starting player scrape cycle.");

                var players = await playerService
                    .GetLeastRecentlyScrapedMembers(BatchSize, MinProfileAge);

                if (players == null || players.Count == 0)
                {
                    _logger.LogInformation("No players to scrape, skipping cycle.");
                }
                else
                {
                    _logger.LogInformation("Checking {Count} player feeds.", players.Count);

                    var profiles = await ScrapePlayersAsync(players, stoppingToken);
                    var achievements = await GetAchievementsToSend(playerService, profiles, stoppingToken);
                    var discordClient = scope.ServiceProvider.GetRequiredService<DiscordSocketClient>();
                    var clanService = scope.ServiceProvider.GetRequiredService<IClanService>();
                    var clans = await clanService.GetClans();
                    await SendAchievements(discordClient, clans, achievements);
                    await playerService.UpdatePlayersFromRuneMetrics(
                        players.Select(x => x.Name).ToList(),
                        profiles);


                    _logger.LogInformation("Finished player scrape cycle.");
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("PlayerScrapeService is stopping due to cancellation.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during player scraping.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));

        _logger.LogInformation("PlayerScrapeService stopped.");
    }

    private async Task SendAchievements(
        DiscordSocketClient discordClient,
        List<Clan> clans,
        Dictionary<Player, List<RuneMetricsActivityDTO>> achievements)
    {
        var groupingByClan = achievements.GroupBy(x => x.Key.ClanId);
        var clanDict = clans.ToDictionary(x => x.Id, x => x);
        foreach (var group in groupingByClan)
        {
            var clanId = group.Key;
            if (clanId == null || !clanDict.ContainsKey(clanId.Value))
            {
                _logger.LogWarning("Player {Player} has unknown clan ID {ClanId}, skipping achievements.",
                    group.First().Key.Name, clanId);
                continue;
            }

            var clan = clanDict[clanId.Value];
            var clanAchievements = group.ToDictionary(x => x.Key, x => x.Value);
            var sb = new StringBuilder();
            foreach (var kvp in clanAchievements)
            {
                var player = kvp.Key;
                var activities = kvp.Value;
                foreach (var activity in activities)
                {
                    string prefix = "";
                    if (ShouldFilterActivity(activity))
                    {
                        prefix = "[To Be Filtered] ";
                    }
                    var message = $"{prefix}{activity.RuneMetricsStringDateToObject().ToString("g")}: {player.Name}: {activity.Details}";
                    sb.AppendLine(message);
                }
            }

            foreach (var guild in clan.Guilds)
            {
                if (guild.AchievementsChannelId == null)
                {
                    _logger.LogWarning("Guild {GuildId} for clan {ClanName} does not have an achievements channel configured, skipping.",
                        guild.Id, clan.Name);
                    continue;
                }

                var discordGuild = discordClient.GetGuild(guild.GuildId);
                if (discordGuild == null)
                {
                    _logger.LogWarning("Discord guild {GuildId} not found for clan {ClanName}, skipping.",
                        guild.GuildId, clan.Name);
                    continue;
                }

                var channel = discordGuild.GetTextChannel(guild.AchievementsChannelId.Value);
                if (channel == null)
                {
                    _logger.LogWarning("Achievements channel {ChannelId} for guild {GuildId} not found, skipping.",
                        guild.AchievementsChannelId.Value, guild.Id);
                    continue;
                }
                await channel.SendMessageAsync(sb.ToString());
            }
        }
    }

    private static readonly string[] FILTER_ACTIVITY_REGEXPS = new[]
    {
        @".*fealty rank.*",
        @".*visited my clan citadel.*",
        @".found a crystal triskelion fragment.*",
        @".*at least (?!200000000(?:\D|$))\d+ experience points.*",
        @".*an abyssal whip.*",
        @".*killed \d+.*",
        @".*killed  .*",
    };
    private bool ShouldFilterActivity(RuneMetricsActivityDTO activity)
    {
        return FILTER_ACTIVITY_REGEXPS.Any(r => Regex.IsMatch(activity.Details.ToLower(), r.ToLower()));
    }

    private async Task<Dictionary<Player, List<RuneMetricsActivityDTO>>> GetAchievementsToSend(
        IPlayerService playerService,
        List<RuneMetricsProfileDTO> profiles,
        CancellationToken cancellationToken)
    {
        var filteredProfiles = profiles.Where(x => string.IsNullOrEmpty(x.Error)).ToList();
        var players = await playerService.GetPlayersByNames(filteredProfiles.Select(x => x.Name).ToList());
        var result = new Dictionary<Player, List<RuneMetricsActivityDTO>>();
        var profileDict = filteredProfiles.ToDictionary(x => x.Name, x => x);
        foreach (var player in players)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (player.MostRecentRuneMetricsEvent == null)
                {
                    _logger.LogInformation("Player {Name} does not have a recorded most recent RuneMetrics event, skipping achievement check.",
                        player.Name);
                    continue;
                }
                var profile = profileDict[player.Name];
                var activities = profile.Activities;
                var newActivities = activities.Where(x => x.RuneMetricsStringDateToObject() > player.MostRecentRuneMetricsEvent).ToList();
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

    private async Task<List<RuneMetricsProfileDTO>> ScrapePlayersAsync(
        List<Player> members,
        CancellationToken cancellationToken)
    {
        var profiles = new List<RuneMetricsProfileDTO>(members.Count);

        for (var i = 0; i < members.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var member = members[i];

            try
            {
                if (PerPlayerDelay > TimeSpan.Zero)
                {
                    await Task.Delay(PerPlayerDelay, cancellationToken);
                }

                _logger.LogInformation(
                    "Scraping player {Index}/{Total} {Name}",
                    i + 1, members.Count, member.Name);

                var profile = await GetPlayerRuneMetricsProfileAsync(member.Name, cancellationToken);

                _logger.LogInformation("Retrieved player feed for {Name}", member.Name);

                profiles.Add(profile);
            }
            catch (FetchRuneMetricsException ex)
            {
                _logger.LogError(ex,
                    "Exception fetching RuneMetrics profile for player {Name}",
                    member.Name);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Scraping cancelled while processing player {Name}.",
                    member.Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unknown exception while processing player {Name}",
                    member.Name);
            }
        }

        return profiles;
    }

    private async Task<RuneMetricsProfileDTO> GetPlayerRuneMetricsProfileAsync(
        string name,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("RuneMetrics"); // optional named client

        var url = $"https://apps.runescape.com/runemetrics/profile/profile?user={name}&activities=20";

        var response = await client.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("Received 429 for {Name}, backing off for {Delay}.", name, TooManyRequestsDelay);
                await Task.Delay(TooManyRequestsDelay, cancellationToken);
            }

            throw new FetchRuneMetricsException($"Received response code {response.StatusCode}");
        }

        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var data = Encoding.UTF8.GetString(content);

        var profile = JsonSerializer.Deserialize<RuneMetricsProfileDTO>(data);

        if (profile is null)
        {
            throw new FetchRuneMetricsException("Unknown error deserializing RuneMetrics profile");
        }

        profile.Name = name;
        profile.ScrapedDate = DateTime.UtcNow;

        return profile;
    }
}
