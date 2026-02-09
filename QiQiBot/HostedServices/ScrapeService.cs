using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QiQiBot.Exceptions;
using QiQiBot.Models;
using QiQiBot.Services;
using System.Text;

namespace QiQiBot.HostedServices
{
    public class ScrapeService : IHostedService, IAsyncDisposable
    {
        private IServiceProvider _serviceProvider;
        private IHttpClientFactory _httpClientFactory;
        ILogger<ScrapeService> _logger;
        public ScrapeService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, ILogger<ScrapeService> logger)
        {
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
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
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error during scraping: {ex}", ex);
                }

            }
        }

        private async Task ScrapeClans(IClanService clanService, List<Clan> clans)
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
                }
                catch (FetchMembersException ex)
                {
                    _logger.LogError("Exception fetching members for clan {name}: {ex}", clan.Name, ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Unknown exception while processing clan {name}: {ex}", clan.Name, ex);
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
