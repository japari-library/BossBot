using System;
using Discord;
using System.Threading.Tasks;
using System.Collections.Generic;
using Discord.WebSocket;

namespace NadekoBot.Core.Services
{
    public interface IMessageLoggerService : INService
    {
        Queue<string> MessageList { get; }

        Task LogUserMessage(SocketUserMessage msg, ITextChannel channel);
    }
}