using Discord;
using QiQiBot.Services;

namespace QiQiBot.BotCommands
{
    /// <summary>
    /// Sets or clears the channel used for clan achievement notifications.
    /// </summary>
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

        public async Task Handle(IBotCommandContext command)
        {
            if (!command.GuildId.HasValue)
            {
                await command.RespondAsync("This command can only be used in a server.");
                return;
            }

            var channel = command.Options.FirstOrDefault()?.Value as IChannel;

            await _clanService.SetAchievementChannel(command.GuildId.Value, channel?.Id);
            var response = channel == null
                ? "Channel for achievements has been cleared."
                : "Channel for achievements has been set.";
            await command.RespondAsync(response);
        }

    }
}
