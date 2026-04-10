using Discord;
using Discord.WebSocket;
using QiQiBot.Exceptions;
using QiQiBot.Services;
using System.Text;

namespace QiQiBot.BotCommands;

/// <summary>
/// Generates an audit report of Discord members without RSNs and RSNs not found in the configured clan.
/// </summary>
internal class ClanRsnAuditCommand(IRsnService rsnService, IClanService clanService, IDiscordSocketClientWrapper client) : IBotCommand
{
    public static string Name => "clan-rsn-audit";

    private readonly IRsnService _rsnService = rsnService;
    private readonly IClanService _clanService = clanService;
    private readonly IDiscordSocketClientWrapper _client = client;

    public static ApplicationCommandProperties BuildCommand()
    {
        var command = new SlashCommandBuilder();
        command.WithName(Name);
        command.WithDescription("Generate RSN audit report for this server");
        command.WithDefaultMemberPermissions(GuildPermission.Administrator);
        return command.Build();
    }

    public async Task Handle(IBotCommandContext command)
    {
        if (!command.GuildId.HasValue)
        {
            await command.RespondAsync("This command can only be used in a server.");
            return;
        }

        var guildId = command.GuildId.Value;
        var discordGuild = _client.GetGuild(guildId);
        if (discordGuild == null)
        {
            await command.RespondAsync("Could not find this Discord server in the bot cache.");
            return;
        }

        var rsnByUserId = await _rsnService.GetRsnsAsync(guildId);

        var usersWithoutRsn = discordGuild.Users
            .Where(u => !u.IsBot && !rsnByUserId.ContainsKey(u.Id))
            .OrderBy(u => u.DisplayName)
            .ToList();

        HashSet<string>? clanMemberNames = null;
        try
        {
            var clan = await _clanService.GetClanAsync(guildId);
            var clanMembers = await _clanService.GetClanMembers(clan.Id);
            clanMemberNames = clanMembers
                .Select(m => m.Name.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
        catch (NoClanRegisteredException)
        {
            clanMemberNames = null;
        }

        var rsnEntries = rsnByUserId
            .Select(kvp => new
            {
                UserId = kvp.Key,
                Rsn = kvp.Value.Trim()
            })
            .OrderBy(x => x.Rsn)
            .ToList();

        var rsnNotInClan = clanMemberNames == null
            ? []
            : rsnEntries.Where(x => !clanMemberNames.Contains(x.Rsn)).ToList();

        var sb = new StringBuilder();

        sb.AppendLine("Users in Discord who have not set their RSN:");
        if (usersWithoutRsn.Count == 0)
        {
            sb.AppendLine("- None");
        }
        else
        {
            foreach (var user in usersWithoutRsn)
            {
                sb.AppendLine($"- {user.Username} ({user.Id})");
            }
        }

        sb.AppendLine();
        sb.AppendLine("Players who have set their RSN, but are not in this guild's configured clan:");
        if (clanMemberNames == null)
        {
            sb.AppendLine("- No clan is configured for this server.");
        }
        else if (rsnNotInClan.Count == 0)
        {
            sb.AppendLine("- None");
        }
        else
        {
            foreach (var entry in rsnNotInClan)
            {
                if (discordGuild.GetUser(entry.UserId) is SocketGuildUser guildUser)
                {
                    sb.AppendLine($"- {entry.Rsn} (Discord: {guildUser.Username}, {entry.UserId})");
                }
                else
                {
                    sb.AppendLine($"- {entry.Rsn} (Discord User ID: {entry.UserId})");
                }
            }
        }

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        await command.RespondWithFileAsync(ms, $"clan_rsn_audit_{DateTime.UtcNow:yyyy-MM-dd}.txt", "Here is the RSN audit report.");
    }
}
