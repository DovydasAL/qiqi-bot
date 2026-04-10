namespace QiQiBot.Services.Notifications;

public interface IMessageBatcher
{
    IEnumerable<string> BatchLines(IEnumerable<string> lines, int maxLines);
}
