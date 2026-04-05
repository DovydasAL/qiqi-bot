using Discord;
using Discord.WebSocket;
using QiQiBot.Services;

namespace QiQiBot.BotCommands
{
    public class ClanSetWelcomeChannel : IBotCommand
    {
        public static string Name => "clan-welcome-channel";

        private IClanService _clanService;

        public ClanSetWelcomeChannel(IClanService clanService)
        {
            _clanService = clanService;
        }

        public static ApplicationCommandProperties BuildCommand()
        {
            var command = new SlashCommandBuilder();
            command.WithName(Name);
            command.WithDescription("Set the channel to post clan welcome events");
            command.WithDefaultMemberPermissions(GuildPermission.Administrator);
            command.AddOption("channel", ApplicationCommandOptionType.Channel, "The channel to post welcome events", isRequired: false);
            return command.Build();
        }

        public async Task Handle(SocketSlashCommand command)
        {
            if (!command.GuildId.HasValue)
            {
                await command.RespondAsync("This command can only be used in a server.");
                return;
            }

            var channelOption = command.Data.Options.FirstOrDefault();
            var channel = channelOption?.Value as SocketChannel;

            await _clanService.SetWelcomeChannel(command.GuildId.Value, channel?.Id);
            var response = channel == null
                ? "Channel for welcome events has been cleared."
                : "Channel for welcome events has been set.";
            await command.RespondAsync(response);
        }
    }
}
