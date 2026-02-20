using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QiQiBot.Exceptions;
using QiQiBot.Models;
using QiQiBot.Services;
using System.Text;

namespace QiQiBot.HostedServices;

public sealed class ClanScrapeService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ClanScrapeService> _logger;

    // scrape interval for all clans
    private static readonly TimeSpan GlobalInterval = TimeSpan.FromMinutes(15);
    // delay between individual clans to avoid hammering API
    private static readonly TimeSpan PerClanDelay = TimeSpan.FromMinutes(2);

    public ClanScrapeService(
        IServiceProvider serviceProvider,
        IHttpClientFactory httpClientFactory,
        ILogger<ClanScrapeService> logger)
    {
        _serviceProvider = serviceProvider;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ClanScrapeService started.");

        // run until app shuts down
        using var timer = new PeriodicTimer(GlobalInterval);

        do
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var clanService = scope.ServiceProvider.GetRequiredService<IClanService>();
                var clanEventService = scope.ServiceProvider.GetRequiredService<IClanEventService>();
                _logger.LogInformation("Starting clan scrape cycle.");

                var clans = await clanService.GetClans();
                _logger.LogInformation("Found {Count} clans.", clans.Count);

                await ScrapeClansAsync(clanService, clanEventService, clans, stoppingToken);

                _logger.LogInformation("Finished clan scrape cycle.");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("ClanScrapeService is stopping due to cancellation.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception during clan scraping.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));

        _logger.LogInformation("ClanScrapeService stopped.");
    }

    private async Task ScrapeClansAsync(
        IClanService clanService,
        IClanEventService clanEventService,
        List<Clan> clans,
        CancellationToken cancellationToken)
    {
        foreach (var clan in clans)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                _logger.LogInformation("Scraping clan {ClanName}.", clan.Name);

                var members = await GetClanMembersFromApiAsync(clan.Name, cancellationToken);

                foreach (var member in members)
                {
                    member.ClanId = clan.Id;
                }
                // TODO: extract this logic to a separate service that computes player leave and joins
                var dbMembersSet = (await clanService.GetClanMembers(clan.Id)).Select(x => x.Name).ToHashSet();
                var apiMemberSet = members.Select(x => x.Name).ToHashSet();
                var playersJoined = new List<string>();
                var playersLeft = new List<string>();
                foreach (var member in dbMembersSet)
                {
                    if (!apiMemberSet.Contains(member))
                    {
                        _logger.LogInformation("Player {PlayerName} left clan {ClanName}.", member, clan.Name);
                        playersLeft.Add(member);
                    }
                }
                foreach (var member in apiMemberSet)
                {
                    if (!dbMembersSet.Contains(member))
                    {
                        _logger.LogInformation("Player {PlayerName} joined clan {ClanName}.", member, clan.Name);
                        playersJoined.Add(member);
                    }
                }

                foreach (var guild in clan.Guilds)
                {
                    if (playersJoined.Count > 0)
                    {
                        await clanEventService.SendPlayerJoinEvent(guild.GuildId, playersJoined);
                    }
                    if (playersLeft.Count > 0)
                    {
                        await clanEventService.SendPlayerLeftEvent(guild.GuildId, playersLeft);
                    }
                }


                _logger.LogInformation("Retrieved {Count} members for clan {ClanName}.",
                    members.Count, clan.Name);

                await clanService.UpdateClanMembers(clan.Id, members);
                await clanService.SetLastScraped(clan.Id, DateTime.UtcNow);

                _logger.LogInformation("Finished scraping clan {ClanName}.", clan.Name);

                // throttle between clans
                if (PerClanDelay > TimeSpan.Zero)
                {
                    await Task.Delay(PerClanDelay, cancellationToken);
                }
            }
            catch (FetchMembersException ex)
            {
                _logger.LogError(ex, "Error fetching members for clan {ClanName}.", clan.Name);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("Scraping cancelled while processing clan {ClanName}.", clan.Name);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unknown error while processing clan {ClanName}.", clan.Name);
            }
        }
    }

    private async Task<List<Player>> GetClanMembersFromApiAsync(
        string clan,
        CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("ClanHiscores"); // optional named client

        var url = $"http://services.runescape.com/m=clan-hiscores/members_lite.ws?clanName={clan}";
        var response = await client.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new FetchMembersException(response.StatusCode);
        }

        var content = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var lines = Encoding.UTF8.GetString(content).Split('\n', StringSplitOptions.RemoveEmptyEntries);

        var result = new List<Player>(capacity: Math.Max(16, lines.Length));

        foreach (var line in lines.Skip(1))
        {
            var splitLine = line.Split(',');
            if (splitLine.Length < 4)
            {
                continue;
            }

            var name = splitLine[0].Replace("\uFFFD", " ");
            if (!long.TryParse(splitLine[2], out var xp))
            {
                continue;
            }

            result.Add(new Player
            {
                Name = name,
                ClanExperience = xp
            });
        }

        return result;
    }
}
