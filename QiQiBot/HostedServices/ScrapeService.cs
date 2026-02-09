using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QiQiBot.Exceptions;
using QiQiBot.Models;
using QiQiBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QiQiBot.HostedServices
{
    public class ScrapeService : IHostedService, IAsyncDisposable
    {
        private IServiceProvider _serviceProvider;
        private IHttpClientFactory _httpClientFactory;
        public ScrapeService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory) 
        {
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine("Starting clan scrape");
                    using var scope = _serviceProvider.CreateScope();
                    var clanService = scope.ServiceProvider.GetRequiredService<IClanService>();
                    var clans = await clanService.GetClans();
                    Console.WriteLine($"Found {clans.Count} clans");
                    await ScrapeClans(clanService, clans);
                    Console.WriteLine("Finished clan scrape");
                    await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during scraping: {ex.Message}");
                }

            }
        }

        private async Task ScrapeClans(IClanService clanService, List<Clan> clans)
        {
            foreach (var clan in clans)
            {
                try
                {
                    Console.WriteLine($"Scraping clan {clan.Name}");
                    var members = await GetClanMembersFromAPI(clan.Name);
                    members.ForEach(m =>
                    {
                        m.ClanId = clan.Id;
                    });
                    Console.WriteLine($"Retrieved {members.Count} members for clan {clan.Name}");
                    await clanService.UpdateClanMembers(clan.Id, members);
                    Console.WriteLine($"Finished scraping clan {clan.Name}");
                }
                catch (FetchMembersException ex)
                {
                    Console.WriteLine($"Exception fetching members for clan {clan.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unknown exception while processing clan {clan.Name}");
                    Console.WriteLine(ex);
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
                Console.WriteLine($"Failed to retrieve members for clan {clan}: {response.StatusCode}");
                throw new FetchMembersException(response.StatusCode);
            }
            var content = await response.Content.ReadAsStringAsync();
            var lines = content.Split("\n");
            var result = new List<ClanMember>();
            foreach (var line in lines.Skip(1))
            {
                var splitLine = line.Split(",");
                if (splitLine.Length < 4)
                {
                    continue;
                }
                var name = splitLine[0];
                var xp = splitLine[2];
                result.Add(new ClanMember() { Name = name, Experience = long.Parse(xp) });
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
