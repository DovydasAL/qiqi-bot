using Discord.WebSocket;

namespace QiQiBot.Services.Notifications;

public interface IDiscordMessageSender
{
    Task SendBatchesAsync(SocketTextChannel channel, IEnumerable<string> batches, CancellationToken ct);
}
