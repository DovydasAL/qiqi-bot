using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QiQiBot.BotCommands;
using QiQiBot.HostedServices;
using QiQiBot.Models;
using QiQiBot.Services;

namespace QiQiBot
{
    internal class Program
    {
        public async static Task Main(string[] args)
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
            var configuration = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json")
                .AddEnvironmentVariables();

            if (builder.Environment.EnvironmentName == "Development")
            {
                configuration.AddUserSecrets<Program>();
            }

            var config = configuration.Build();
            builder.Services.AddSingleton<IConfiguration>(config);
            builder.Services.AddHostedService<BotService>();
            builder.Services.AddHostedService<ScrapeService>();
            builder.Services.AddSingleton<DiscordSocketClient>();
            builder.Services.AddScoped<IClanService, ClanService>();
            builder.Services.AddScoped<ClanActivityCommand>();
            builder.Services.AddScoped<ClanRegisterCommand>();
            builder.Services.AddHttpClient();
            builder.Services.AddDbContextPool<ClanContext>(opt =>
                opt.UseNpgsql("Server=localhost;Port=5432;Database=QiQiBot;User Id=postgres;Password=postgres;"));
            IHost host = builder.Build();
            host.Run();
        }
    }
}
