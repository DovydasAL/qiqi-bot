using Discord.WebSocket;

namespace QiQiBot.Services.Notifications;

public interface INotificationChannelResolver
{
    SocketTextChannel? ResolveTextChannel(ulong guildId, ulong channelId, string notificationType);
}
