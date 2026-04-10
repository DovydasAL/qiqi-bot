using Discord;
using QiQiBot.Services;

namespace QiQiBot.BotCommands
{
    /// <summary>
    /// Sets or clears the channel used for clan member leave and join event notifications.
    /// </summary>
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

        public async Task Handle(IBotCommandContext command)
        {
            if (!command.GuildId.HasValue)
            {
                await command.RespondAsync("This command can only be used in a server.");
                return;
            }

            var channel = command.Options.FirstOrDefault()?.Value as IChannel;

            await _clanService.SetLeaveJoinChannel(command.GuildId.Value, channel?.Id);
            var response = channel == null
                ? "Channel for leave and join events has been cleared."
                : "Channel for leave and join events has been set.";
            await command.RespondAsync(response);
        }
    }
}
