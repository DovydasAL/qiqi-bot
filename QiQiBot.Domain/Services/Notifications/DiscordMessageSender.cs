using Discord.WebSocket;

namespace QiQiBot.Services.Notifications;

public sealed class DiscordMessageSender : IDiscordMessageSender
{
    public async Task SendBatchesAsync(SocketTextChannel channel, IEnumerable<string> batches, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(channel);
        ArgumentNullException.ThrowIfNull(batches);

        foreach (var batch in batches)
        {
            ct.ThrowIfCancellationRequested();
            await channel.SendMessageAsync(batch);
        }
    }
}
