using Discord;
using Discord.WebSocket;
using QiQiBot.Exceptions;
using QiQiBot.Models;
using QiQiBot.Services;
using System.Text;

namespace QiQiBot.BotCommands
{
    internal class ClanActivityCommand : IBotCommand
    {
        public static string Name => "clan-activity";
        private IClanService _clanService;

        public ClanActivityCommand(IClanService clanService)
        {
            _clanService = clanService;
        }

        public static ApplicationCommandProperties BuildCommand()
        {
            var command = new SlashCommandBuilder();
            command.WithName(Name);
            command.WithDescription("View activity for members in clan");
            return command.Build();
        }

        public async Task Handle(SocketSlashCommand command)
        {
            if (!command.GuildId.HasValue)
            {
                await command.RespondAsync("This command can only be used in a server.");
                return;
            }

            Clan clan = null;
            try
            {
                clan = await _clanService.GetClanAsync(command.GuildId.Value);
            }
            catch (NoClanRegisteredException ex)
            {
                await command.RespondAsync("No clan has been set for this server. Use `/clan-register` to set the clan for this server.");
                return;
            }
            var clanMembers = await _clanService.GetClanMembers(clan.Id);

            var membersWithActivity = clanMembers
                .Select(m =>
                {
                    DateTime? activityDate = null;
                    if (m.LastClanExperienceUpdate.HasValue && m.MostRecentRuneMetricsEvent.HasValue)
                    {
                        activityDate = m.LastClanExperienceUpdate > m.MostRecentRuneMetricsEvent
                            ? m.LastClanExperienceUpdate
                            : m.MostRecentRuneMetricsEvent;
                    }
                    else if (m.LastClanExperienceUpdate.HasValue)
                    {
                        activityDate = m.LastClanExperienceUpdate;
                    }
                    else if (m.MostRecentRuneMetricsEvent.HasValue)
                    {
                        activityDate = m.MostRecentRuneMetricsEvent;
                    }

                    return new
                    {
                        m.Name,
                        ActivityDate = activityDate
                    };
                })
                .OrderBy(x => x.ActivityDate.HasValue ? 0 : 1)   // non-null first
                .ThenBy(x => x.ActivityDate)                     // oldest to newest; use .ThenByDescending for newest first
                .ThenBy(x => x.Name)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Name,Last Active");

            foreach (var m in membersWithActivity)
            {
                var lastActiveStr = m.ActivityDate?.ToShortDateString() ?? "Unknown";
                sb.AppendLine($"{m.Name},{lastActiveStr}");
            }
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            await command.RespondWithFileAsync(ms, $"clan_activity_{DateTime.UtcNow.ToString("yyyy-MM-dd")}.csv", "Here is the clan activity report.");
        }
    }
}
