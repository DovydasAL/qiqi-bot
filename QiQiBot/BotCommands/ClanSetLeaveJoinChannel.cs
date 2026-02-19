using Discord;
using Discord.WebSocket;
using QiQiBot.Services;

namespace QiQiBot.BotCommands
{
    public class ClanSetLeaveJoinChannel : IBotCommand
    {
        public static string Name => "clan-leave-join-channel";

        private IClanService _clanService;

        public ClanSetLeaveJoinChannel(IClanService clanService)
        {
            _clanService = clanService;
        }

        public static ApplicationCommandProperties BuildCommand()
        {
            var command = new SlashCommandBuilder();
            command.WithName(Name);
            command.WithDescription("Set the channel to post clan member leave and join events");
            command.WithDefaultMemberPermissions(GuildPermission.Administrator);
            command.AddOption("channel", ApplicationCommandOptionType.Channel, "The channel to post leave and join events", isRequired: false);
            return command.Build();
        }

        public async Task Handle(SocketSlashCommand command)
        {
            if (!command.GuildId.HasValue)
            {
                await command.RespondAsync("This command can only be used in a server.");
                return;
            }

            var channel = (SocketChannel)command.Data.Options.First().Value;
            await _clanService.SetAchievementChannel(command.GuildId.Value, channel?.Id);
            await command.RespondAsync($"Channel for leave and join events has been set.");
        }
    }
}
