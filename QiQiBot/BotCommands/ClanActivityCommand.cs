using Discord;
using Discord.WebSocket;
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

            var clan = await _clanService.GetClanAsync(command.GuildId.Value);
            var clanMembers = await _clanService.GetClanMembers(clan.Id);
            var sb = new StringBuilder();
            sb.AppendLine($"Name,Last Active");
            var notNullSorted = clanMembers.Where(x => x.LastClanExperienceUpdate.HasValue).OrderBy(x => x.LastClanExperienceUpdate).ToList();
            foreach (var member in notNullSorted)
            {
                sb.AppendLine($"{member.Name},{member.LastClanExperienceUpdate.Value.ToShortDateString()}");
            }
            var nullSorted = clanMembers.Where(x => !x.LastClanExperienceUpdate.HasValue).OrderBy(x => x.Name).ToList();
            foreach (var member in nullSorted)
            {
                sb.AppendLine($"{member.Name},Unknown");
            }
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            await command.RespondWithFileAsync(ms, $"clan_activity_{DateTime.UtcNow.ToString("yyyy-MM-dd")}.csv", "Here is the clan activity report.");
        }
    }
}
