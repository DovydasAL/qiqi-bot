using Discord;
using Discord.WebSocket;
using QiQiBot.Exceptions;
using QiQiBot.Models;
using QiQiBot.Services;
using System.Text;

namespace QiQiBot.BotCommands
{
    public class ClanCappedCommand : IBotCommand
    {
        public static string Name => "clan-capped";
        private IClanService _clanService;

        public ClanCappedCommand(IClanService clanService)
        {
            _clanService = clanService;
        }

        public static ApplicationCommandProperties BuildCommand()
        {
            var command = new SlashCommandBuilder();
            command.WithName(Name);
            command.WithDescription("View who capped since last reset");
            return command.Build();
        }

        public async Task Handle(SocketSlashCommand command)
        {
            if (!command.GuildId.HasValue)
            {
                await command.RespondAsync("This command can only be used in a server.");
                return;
            }

            Guild guild = null;
            try
            {
                guild = await _clanService.GetGuild(command.GuildId.Value);
            }
            catch (NoClanRegisteredException ex)
            {
                await command.RespondAsync("No clan has been set for this server. Use `/clan-register` to set the clan for this server.");
                return;
            }
            if (guild.ClanId == null)
            {
                await command.RespondAsync("No clan has been set for this server. Use `/clan-register` to set the clan for this server.");
                return;
            }
            if (guild.CapResetDay == null || guild.CapResetTime == null)
            {
                await command.RespondAsync("Cap reset day and time have not been set for this server. Use `/clan-citadel-reset` to set the cap reset day and time for this server.");
                return;
            }
            // 0-6 for Sunday-Saturday
            var clanCapDay = guild.CapResetDay.Value;
            if (clanCapDay < 0 || clanCapDay > 6)
            {
                await command.RespondAsync("Cap reset day is invalid for this server. Use `/clan-citadel-reset` to set the cap reset day and time again.");
                return;
            }
            // 00:00-23:59 for the time of day
            var clanCapTime = guild.CapResetTime;
            if (!TimeSpan.TryParse(clanCapTime, out var clanCapTimeOfDay))
            {
                await command.RespondAsync("Cap reset time is invalid for this server. Use `/clan-citadel-reset` to set the cap reset day and time again.");
                return;
            }

            var nowUtc = DateTime.UtcNow;
            var resetDayOfWeek = (DayOfWeek)clanCapDay;
            var daysSinceResetDay = ((int)nowUtc.DayOfWeek - (int)resetDayOfWeek + 7) % 7;
            var lastReset = nowUtc.Date.AddDays(-daysSinceResetDay).Add(clanCapTimeOfDay);
            if (nowUtc < lastReset)
            {
                lastReset = lastReset.AddDays(-7);
            }

            var clanMembers = await _clanService.GetClanMembers(guild.ClanId.Value);
            var membersWhoCapped = new List<Player>();

            foreach (var member in clanMembers)
            {
                var lastCap = member.LastCapped;
                var hasCapped = lastCap.HasValue && lastCap.Value >= lastReset;
                if (hasCapped)
                {
                    membersWhoCapped.Add(member);
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("Name,Capped");
            foreach (var member in membersWhoCapped.OrderBy(x => x.LastCapped))
            {
                sb.AppendLine($"{member.Name},{member.LastCapped.Value.ToString("g")}");
            }
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            await command.RespondWithFileAsync(ms, $"clan_capped_{DateTime.UtcNow.ToString("yyyy-MM-dd")}.csv", "Here are the members who've capped this reset");
        }
    }
}
