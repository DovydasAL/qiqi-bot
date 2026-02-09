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
    internal class ClanRegisterCommand : IBotCommand
    {
        public static string Name => "clan-register";

        private IClanService _clanService;

        public ClanRegisterCommand(IClanService clanService)
        {
            _clanService = clanService;
        }

        public static ApplicationCommandProperties BuildCommand()
        {
            var command = new SlashCommandBuilder();
            command.WithName(Name);
            command.WithDescription("Register a clan for this server");
            command.WithDefaultMemberPermissions(GuildPermission.Administrator);
            command.AddOption("clan_name", ApplicationCommandOptionType.String, "The name of the clan to register to this server", isRequired: true);
            return command.Build();
        }

        public async Task Handle(SocketSlashCommand command)
        {
            if (!command.GuildId.HasValue)
            {
                await command.RespondAsync("This command can only be used in a server.");
                return;
            }

            var clanName = command.Data.Options.First().Value.ToString();
            if (string.IsNullOrEmpty(clanName) || clanName.Length > 20)
            {
                await command.RespondAsync("Clan name cannot be empty and must be 20 characters or less.");
                return;
            }
            var guildId = command.GuildId.Value;
            await _clanService.RegisterClan(clanName, guildId);
        }
    }
}
