using Discord;
using Discord.WebSocket;
using QiQiBot.Services;

namespace QiQiBot.BotCommands
{
    public class ClanSetCitadelResetCommand : IBotCommand
    {
        public static string Name => "clan-citadel-reset";

        private IClanService _clanService;

        public ClanSetCitadelResetCommand(IClanService clanService)
        {
            _clanService = clanService;
        }

        public static ApplicationCommandProperties BuildCommand()
        {
            var command = new SlashCommandBuilder();
            command.WithName(Name);
            command.WithDescription("Set clan citadel reset time");
            command.WithDefaultMemberPermissions(GuildPermission.Administrator);
            command.AddOption(new SlashCommandOptionBuilder()
                .WithName("day")
                .WithType(ApplicationCommandOptionType.Integer)
                .WithDescription("The day for reset")
                .WithRequired(true)
                .AddChoice("Sunday", 0)
                .AddChoice("Monday", 1)
                .AddChoice("Tuesday", 2)
                .AddChoice("Wednesday", 3)
                .AddChoice("Thursday", 4)
                .AddChoice("Friday", 5)
                .AddChoice("Saturday", 6));

            command.AddOption(new SlashCommandOptionBuilder()
                .WithName("time")
                .WithType(ApplicationCommandOptionType.String)
                .WithDescription("The game time of reset (00:00-23:30)")
                .WithRequired(true));

            return command.Build();
        }

        public async Task Handle(SocketSlashCommand command)
        {
            if (!command.GuildId.HasValue)
            {
                await command.RespondAsync("This command can only be used in a server.");
                return;
            }

            var day = (long)command.Data.Options.First().Value;
            var time = command.Data.Options.Last().Value.ToString();
            Console.WriteLine($"Received citadel reset command with day: {day}, time: {time}");
            await _clanService.SetCitadelResetTime(command.GuildId.Value, day, time);
            await command.RespondAsync($"Citadel reset time has been set to {(DayOfWeek)day} at {time}.");
        }
    }
}
