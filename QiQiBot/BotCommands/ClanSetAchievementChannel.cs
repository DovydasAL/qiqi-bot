using Discord;
using Discord.WebSocket;
using QiQiBot.Services;

namespace QiQiBot.BotCommands
{
    public class ClanSetAchievementChannel : IBotCommand
    {
        public static string Name => "clan-achievement-channel";

        private IClanService _clanService;

        public ClanSetAchievementChannel(IClanService clanService)
        {
            _clanService = clanService;
        }

        public static ApplicationCommandProperties BuildCommand()
        {
            var command = new SlashCommandBuilder();
            command.WithName(Name);
            command.WithDescription("Set the channel to post member achievements");
            command.WithDefaultMemberPermissions(GuildPermission.Administrator);
            command.AddOption("channel", ApplicationCommandOptionType.Channel, "The channel to post achievements", isRequired: false);
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
            await command.RespondAsync($"Channel for achievements has been set.");
        }

    }
}
