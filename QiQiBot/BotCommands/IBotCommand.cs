using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QiQiBot.BotCommands
{
    internal interface IBotCommand
    {
        static abstract string Name { get; }
        static abstract ApplicationCommandProperties BuildCommand();
        Task Handle(SocketSlashCommand command);
    }
}
