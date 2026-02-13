using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QiQiBot.Exceptions;
using QiQiBot.Models;
using QiQiBot.Services;
using System.Text;
using System.Text.Json;

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
