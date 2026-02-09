using Discord;
using Discord.WebSocket;

namespace QiQiBot.BotCommands
{
    internal interface IBotCommand
    {
        static abstract string Name { get; }
        static abstract ApplicationCommandProperties BuildCommand();
        Task Handle(SocketSlashCommand command);
    }
}
