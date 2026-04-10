using Discord;

namespace QiQiBot.BotCommands
{
    public interface IBotCommand
    {
        static abstract string Name { get; }
        static abstract ApplicationCommandProperties BuildCommand();
        Task Handle(IBotCommandContext command);
    }
}
