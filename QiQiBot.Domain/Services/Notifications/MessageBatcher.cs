namespace QiQiBot.Services.Notifications;

public sealed class MessageBatcher : IMessageBatcher
{
    public IEnumerable<string> BatchLines(IEnumerable<string> lines, int maxLines)
    {
        ArgumentNullException.ThrowIfNull(lines);

        if (maxLines <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLines), "Value must be greater than zero.");
        }

        return lines
            .Chunk(maxLines)
            .Select(batch => string.Join(Environment.NewLine, batch));
    }
}
