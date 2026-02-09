using Discord;
using Discord.WebSocket;
using QiQiBot.Services;
using System;
using System.Collections.Generic;
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
            sb.AppendLine($"Clan Member Activity for guild: {command.GuildId.Value}");
            foreach (var member in clanMembers)
            {
                // Placeholder for actual clan member activity retrieval logic
                sb.AppendLine($"{member}: Last active on {member.LastExperienceUpdate.ToShortDateString()}");
            }
            await command.RespondAsync(sb.ToString());
        }
    }
}
