using Discord;
using Discord.WebSocket;
using QiQiBot.Services;

namespace QiQiBot.BotCommands;

internal class RsnCommand(IRsnService rsnService, DiscordSocketClient client) : IBotCommand
{
    public static string Name => "rsn";

    private readonly IRsnService _rsnService = rsnService;
    private readonly DiscordSocketClient _client = client;

    public static ApplicationCommandProperties BuildCommand()
    {
        var command = new SlashCommandBuilder();
        command.WithName(Name);
        command.WithDescription("Link your RuneScape display name to this server");
        command.AddOption("name", ApplicationCommandOptionType.String, "Your RuneScape display name", isRequired: true);
        return command.Build();
    }

    public async Task Handle(SocketSlashCommand command)
    {
        if (!command.GuildId.HasValue)
        {
            await command.RespondAsync("This command can only be used in a server.");
            return;
        }

        var providedName = command.Data.Options.FirstOrDefault()?.Value?.ToString()?.Trim();
        if (string.IsNullOrWhiteSpace(providedName) || providedName.Length > 12)
        {
            await command.RespondAsync("RuneScape names must be between 1 and 12 characters.", ephemeral: true);
            return;
        }

        var guildId = command.GuildId.Value;
        var userId = command.User.Id;
        var previousName = await _rsnService.GetRsnAsync(guildId, userId);
        await _rsnService.SetRsnAsync(guildId, userId, providedName);

        if (previousName is null)
        {
            await command.RespondAsync($"Your RuneScape name has been set to {providedName}. Your Discord nickname has also been changed for this server.", ephemeral: true);
        }
        else if (previousName.Equals(providedName, StringComparison.Ordinal))
        {
            await command.RespondAsync($"Your RuneScape name is already set to {providedName}.", ephemeral: true);
        }
        else
        {
            await command.RespondAsync($"Updated your RuneScape name from {previousName} to {providedName}. Your Discord nickname has also been changed for this server.", ephemeral: true);
        }
        var guild = _client.GetGuild(guildId);
        var guildUser = guild.GetUser(userId);
        await guildUser.ModifyAsync(x => x.Nickname = providedName);
    }
}
