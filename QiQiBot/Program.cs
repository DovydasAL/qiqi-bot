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
using QiQiBot.Services.Abstractions;
using QiQiBot.Services.Notifications;

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
            builder.Services.AddHostedService<ClanScrapeService>();
            builder.Services.AddHostedService<PlayerScrapeService>();
            builder.Services.AddSingleton<IDiscordSocketClientWrapper>(sp =>
            {
                var socketConfig = new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.All,
                };

                var client = new DiscordSocketClient(socketConfig);
                return new DiscordSocketClientWrapper(client);
            });
            builder.Services.AddScoped<INotificationChannelResolver, NotificationChannelResolver>();
            builder.Services.AddScoped<IMessageBatcher, MessageBatcher>();
            builder.Services.AddScoped<IDiscordMessageSender, DiscordMessageSender>();
            builder.Services.AddScoped<IClanRegistrationService, ClanRegistrationService>();
            builder.Services.AddScoped<IGuildConfigurationService, GuildConfigurationService>();
            builder.Services.AddScoped<IClanQueryService, ClanQueryService>();
            builder.Services.AddScoped<IClanMembershipService, ClanMembershipService>();
            builder.Services.AddScoped<IClanService, ClanService>();
            builder.Services.AddScoped<IPlayerService, PlayerService>();
            builder.Services.AddScoped<IClanEventService, ClanEventService>();
            builder.Services.AddScoped<IAchievementService, AchievementService>();
            builder.Services.AddScoped<ICitadelActivityService, CitadelActivityService>();
            builder.Services.AddScoped<IRsnService, RsnService>();
            builder.Services.AddScoped<ClanActivityCommand>();
            builder.Services.AddScoped<ClanRegisterCommand>();
            builder.Services.AddScoped<ClanSetAchievementChannel>();
            builder.Services.AddScoped<ClanSetCitadelChannel>();
            builder.Services.AddScoped<ClanSetCitadelResetCommand>();
            builder.Services.AddScoped<ClanCappedCommand>();
            builder.Services.AddScoped<ClanSetLeaveJoinChannel>();
            builder.Services.AddScoped<ClanSetWelcomeChannel>();
            builder.Services.AddScoped<ClanDebugCommand>();
            builder.Services.AddScoped<ClanRsnAuditCommand>();
            builder.Services.AddScoped<RsnCommand>();
            builder.Services.AddScoped<RsnSetCommand>();
            builder.Services.AddHttpClient();
            builder.Services.AddDbContextPool<ClanContext>(opt =>
            {
                opt.UseNpgsql(config.GetValue<string>("DBConnectionString"), o => o.MigrationsHistoryTable("__migrations", "qiqi"));

            });
            IHost host = builder.Build();
            host.Run();
        }
    }
}
