using Discord;
using Discord.WebSocket;
using QiQiBot.Services;

namespace QiQiBot.BotCommands;

/// <summary>
/// Allows administrators to set or update another member's RuneScape display name for the server.
/// </summary>
internal class RsnSetCommand(IRsnService rsnService, IDiscordSocketClientWrapper client) : IBotCommand
{
    public static string Name => "rsn-set";

    private readonly IRsnService _rsnService = rsnService;
    private readonly IDiscordSocketClientWrapper _client = client;

    public static ApplicationCommandProperties BuildCommand()
    {
        var command = new SlashCommandBuilder();
        command.WithName(Name);
        command.WithDescription("Set a player's RuneScape display name for this server");
        command.WithDefaultMemberPermissions(GuildPermission.Administrator);
        command.AddOption("user", ApplicationCommandOptionType.User, "The Discord user to set the RuneScape name for", isRequired: true);
        command.AddOption("name", ApplicationCommandOptionType.String, "The RuneScape display name", isRequired: true);
        return command.Build();
    }

    public async Task Handle(IBotCommandContext command)
    {
        if (!command.GuildId.HasValue)
        {
            await command.RespondAsync("This command can only be used in a server.", ephemeral: true);
            return;
        }

        var userOption = command.Options.FirstOrDefault(x => x.Name == "user");
        var nameOption = command.Options.FirstOrDefault(x => x.Name == "name");

        var targetUser = userOption?.Value as SocketGuildUser;
        var providedName = nameOption?.Value?.ToString()?.Trim();

        if (targetUser is null)
        {
            await command.RespondAsync("Please provide a valid server member.", ephemeral: true);
            return;
        }

        if (string.IsNullOrWhiteSpace(providedName) || providedName.Length > 12)
        {
            await command.RespondAsync("RuneScape names must be between 1 and 12 characters.", ephemeral: true);
            return;
        }

        var guildId = command.GuildId.Value;
        var previousName = await _rsnService.GetRsnAsync(guildId, targetUser.Id);
        await _rsnService.SetRsnAsync(guildId, targetUser.Id, providedName);

        if (previousName is null)
        {
            await command.RespondAsync($"Set {targetUser.Mention}'s RuneScape name to {providedName}. Their Discord nickname has also been changed for this server.", ephemeral: true);
        }
        else if (previousName.Equals(providedName, StringComparison.Ordinal))
        {
            await command.RespondAsync($"{targetUser.Mention}'s RuneScape name is already set to {providedName}.", ephemeral: true);
        }
        else
        {
            await command.RespondAsync($"Updated {targetUser.Mention}'s RuneScape name from {previousName} to {providedName}. Their Discord nickname has also been changed for this server.", ephemeral: true);
        }

        var guild = _client.GetGuild(guildId);
        var guildUser = guild?.GetUser(targetUser.Id);
        if (guildUser is not null)
        {
            await guildUser.ModifyAsync(x => x.Nickname = providedName);
        }
    }
}
