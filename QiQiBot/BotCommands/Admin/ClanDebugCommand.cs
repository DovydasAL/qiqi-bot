using Discord;
using QiQiBot.Services;

namespace QiQiBot.BotCommands
{
    /// <summary>
    /// Enables or disables debug mode for the current Discord server.
    /// </summary>
    public class ClanDebugCommand : IBotCommand
    {
        public static string Name => "clan-debug";

        private readonly IClanService _clanService;

        public ClanDebugCommand(IClanService clanService)
        {
            _clanService = clanService;
        }

        public static ApplicationCommandProperties BuildCommand()
        {
            var builder = new SlashCommandBuilder();
            builder.WithName(Name);
            builder.WithDescription("Enable or disable debug mode for this guild");
            builder.WithDefaultMemberPermissions(GuildPermission.Administrator);
            builder.AddOption(new SlashCommandOptionBuilder()
                .WithName("enabled")
                .WithDescription("Whether to enable debug mode")
                .WithType(ApplicationCommandOptionType.Boolean)
                .WithRequired(true));
            return builder.Build();
        }

        public async Task Handle(IBotCommandContext command)
        {
            if (!command.GuildId.HasValue)
            {
                await command.RespondAsync("This command can only be used in a server.");
                return;
            }

            var enabled = (bool)command.Options.First().Value!;
            await _clanService.SetDebugMode(command.GuildId.Value, enabled);
            await command.RespondAsync($"Clan debug mode has been {(enabled ? "enabled" : "disabled")}.");
        }
    }
}
