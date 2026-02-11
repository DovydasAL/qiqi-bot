using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QiQiBot.Exceptions;
using QiQiBot.Models;
using QiQiBot.Services;
using System.Text;
using System.Text.Json;

namespace QiQiBot.HostedServices
{
    public class PlayerScrapeService : IHostedService, IAsyncDisposable
    {
        private IServiceProvider _serviceProvider;
        ILogger<PlayerScrapeService> _logger;
        private IHttpClientFactory _httpClientFactory;
        public PlayerScrapeService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, ILogger<PlayerScrapeService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("StartAsync for PlayerScrapeService");
            Task.Run(async () =>
             {
                 while (!cancellationToken.IsCancellationRequested)
                 {
                     try
                     {
                         _logger.LogInformation("Starting player scrape");
                         using var scope = _serviceProvider.CreateScope();
                         var playerService = scope.ServiceProvider.GetRequiredService<IPlayerService>();
                         var players = await playerService.GetLeastRecentlyScrapedMembers(60, TimeSpan.FromMinutes(60));
                         if (players == null || players.Count == 0)
                         {
                             _logger.LogInformation("No players to scrape, skipping");
                             await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);
                             continue;
                         }
                         _logger.LogInformation($"Checking {players.Count} player feeds");
                         var profiles = await ScrapePlayers(players, cancellationToken);
                         await playerService.UpdatePlayersFromRuneMetrics(players.Select(x => x.Name).ToList(), profiles);
                         _logger.LogInformation("Finished player scrape");
                     }
                     catch (OperationCanceledException)
                     {
                         _logger.LogInformation("Player scrape service is stopping due to cancellation.");
                         return;
                     }
                     catch (Exception ex)
                     {
                         _logger.LogError(ex, "Error during player scraping");
                     }
                     await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);


                 }
             }, cancellationToken);
            return Task.CompletedTask;
        }

        private async Task<List<RuneMetricsProfileDTO>> ScrapePlayers(List<ClanMember> members, CancellationToken cancellationToken = default)
        {
            List<RuneMetricsProfileDTO> profiles = new List<RuneMetricsProfileDTO>();
            foreach (var member in members)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    _logger.LogInformation($"Scraping player {member.Name}");
                    var profile = await GetPlayerRuneMetricsProfile(member.Name);
                    _logger.LogInformation($"Retrieved player feed for {member.Name}");
                    profiles.Add(profile);
                }
                catch (FetchRuneMetricsException ex)
                {
                    _logger.LogError(ex, "Exception fetching RuneMetrics profile for player {name}", member.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unknown exception while processing player {name}", member.Name);
                }
            }
            return profiles;
        }

        private async Task<RuneMetricsProfileDTO> GetPlayerRuneMetricsProfile(string name)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://apps.runescape.com/runemetrics/profile/profile?user={name}&activities=20";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
                throw new FetchRuneMetricsException($"Received response code {response.StatusCode}");
            }
            var content = await response.Content.ReadAsByteArrayAsync();
            var data = Encoding.UTF8.GetString(content);
            var profile = JsonSerializer.Deserialize<RuneMetricsProfileDTO>(data);
            if (profile == null)
            {
                throw new FetchRuneMetricsException("Unknown error deserializing RuneMetrics profile");
            }
            profile.Name = name;

            return profile;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        }

        public async ValueTask DisposeAsync()
        {
        }
    }
}
