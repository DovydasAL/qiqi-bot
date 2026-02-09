using Discord;
using Discord.WebSocket;
using QiQiBot.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            command.WithDefaultMemberPermissions(GuildPermission.Administrator);
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
            var sortedMembers = clanMembers.OrderBy(x => x.LastExperienceUpdate).ToList();
            foreach (var member in sortedMembers)
            {
                sb.AppendLine($"{member.Name},{member.LastExperienceUpdate.ToShortDateString()}");
            }
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            await command.RespondWithFileAsync(ms, $"clan_activity_{DateTime.UtcNow.ToString("yyyy-mm-dd")}.csv", "Here is the clan activity report.");
        }
    }
}
