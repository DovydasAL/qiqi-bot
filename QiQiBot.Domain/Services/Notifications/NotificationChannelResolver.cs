using Discord.WebSocket;
using Microsoft.Extensions.Logging;

namespace QiQiBot.Services.Notifications;

public sealed class NotificationChannelResolver : INotificationChannelResolver
{
    private readonly IDiscordSocketClientWrapper _discordClient;
    private readonly ILogger<NotificationChannelResolver> _logger;

    public NotificationChannelResolver(
        IDiscordSocketClientWrapper discordClient,
        ILogger<NotificationChannelResolver> logger)
    {
        _discordClient = discordClient;
        _logger = logger;
    }

    public SocketTextChannel? ResolveTextChannel(ulong guildId, ulong channelId, string notificationType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notificationType);

        var guild = _discordClient.GetGuild(guildId);
        if (guild == null)
        {
            _logger.LogWarning(
                "Discord guild {GuildId} not found for {NotificationType} notifications, skipping.",
                guildId,
                notificationType);
            return null;
        }

        var channel = guild.GetTextChannel(channelId);
        if (channel == null)
        {
            _logger.LogWarning(
                "{NotificationType} channel {ChannelId} not found in Discord guild {GuildId}, skipping.",
                notificationType,
                channelId,
                guildId);
            return null;
        }

        return channel;
    }
}
