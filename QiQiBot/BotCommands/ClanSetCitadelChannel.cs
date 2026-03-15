using Discord;
using Discord.WebSocket;
using QiQiBot.Services;

namespace QiQiBot.BotCommands
{
    public class ClanSetCitadelChannel : IBotCommand
    {
        public static string Name => "clan-citadel-channel";

        private readonly IClanService _clanService;

        public ClanSetCitadelChannel(IClanService clanService)
        {
            _clanService = clanService;
        }

        public static ApplicationCommandProperties BuildCommand()
        {
            var command = new SlashCommandBuilder();
            command.WithName(Name);
            command.WithDescription("Set the channel to post citadel visit/cap notifications");
            command.WithDefaultMemberPermissions(GuildPermission.Administrator);
            command.AddOption("channel", ApplicationCommandOptionType.Channel, "The channel to post citadel notifications", isRequired: false);
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

            await _clanService.SetCitadelChannel(command.GuildId.Value, channel?.Id);
            var response = channel == null
                ? "Channel for citadel notifications has been cleared."
                : "Channel for citadel notifications has been set.";
            await command.RespondAsync(response);
        }
    }
}
