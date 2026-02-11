using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QiQiBot.Exceptions;
using QiQiBot.Models;
using QiQiBot.Services;
using System.Text;

namespace QiQiBot.HostedServices
{
    public class ClanScrapeService : IHostedService, IAsyncDisposable
    {
        private IServiceProvider _serviceProvider;
        private IHttpClientFactory _httpClientFactory;
        ILogger<ClanScrapeService> _logger;
        public ClanScrapeService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, ILogger<ClanScrapeService> logger)
        {
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StartAsync for ClanScrapeService");
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        _logger.LogInformation("Starting clan scrape");
                        using var scope = _serviceProvider.CreateScope();
                        var clanService = scope.ServiceProvider.GetRequiredService<IClanService>();
                        var clans = await clanService.GetClans();
                        _logger.LogInformation($"Found {clans.Count} clans");
                        await ScrapeClans(clanService, clans);
                        _logger.LogInformation("Finished clan scrape");
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("Scrape service is stopping due to cancellation.");
                        return;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during scraping");
                    }
                    await Task.Delay(TimeSpan.FromMinutes(120), cancellationToken);


                }
            }, cancellationToken);
            return Task.CompletedTask;
        }

        private async Task ScrapeClans(IClanService clanService, List<Clan> clans, CancellationToken cancellationToken = default)
        {
            foreach (var clan in clans)
            {
                try
                {
                    _logger.LogInformation($"Scraping clan {clan.Name}");
                    var members = await GetClanMembersFromAPI(clan.Name);
                    members.ForEach(m =>
                    {
                        m.ClanId = clan.Id;
                    });
                    _logger.LogInformation($"Retrieved {members.Count} members for clan {clan.Name}");
                    await clanService.UpdateClanMembers(clan.Id, members);
                    await clanService.SetLastScraped(clan.Id, DateTime.UtcNow);
                    _logger.LogInformation($"Finished scraping clan {clan.Name}");
                    await Task.Delay(TimeSpan.FromMinutes(2), cancellationToken);
                }
                catch (FetchMembersException ex)
                {
                    _logger.LogError(ex, "Exception fetching members for clan {name}", clan.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unknown exception while processing clan {name}", clan.Name);
                }
            }
        }

        private async Task<List<ClanMember>> GetClanMembersFromAPI(string clan)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"http://services.runescape.com/m=clan-hiscores/members_lite.ws?clanName={clan}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new FetchMembersException(response.StatusCode);
            }
            var content = await response.Content.ReadAsByteArrayAsync();
            var lines = Encoding.UTF8.GetString(content).Split("\n");
            var result = new List<ClanMember>();
            foreach (var line in lines.Skip(1))
            {
                var splitLine = line.Split(",");
                if (splitLine.Length < 4)
                {
                    continue;
                }
                var name = splitLine[0].Replace("\uFFFD", " ");
                var xp = splitLine[2];
                result.Add(new ClanMember() { Name = name, ClanExperience = long.Parse(xp) });
            }
            return result;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }

        public async ValueTask DisposeAsync()
        {
        }
    }
}
