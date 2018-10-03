using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using NadekoBot.Core.Services;
using System.Text.RegularExpressions;


namespace NadekoBot.Core.Services
{
	public class MessageLoggerService : IMessageLoggerService
    {
        private readonly int MAX_LOGGED_MESSAGES = 500; //technically not a magic number if i store it in a variable
        private Queue<string> messageList; //messages will be stored here

        public MessageLoggerService()
        {
            messageList = new Queue<string>();
        }

        public Queue<string> MessageList
        {
            get { return messageList; }
        }

		public Task LogUserMessage(SocketUserMessage msg, ITextChannel channel)
        {
            string content = Regex.Replace(msg.Content, "<(.*?[0-9]*)?>", " "); //remove emotes from message

            if (channel.IsNsfw || content == "") return Task.FromResult(0); //let's not log messages from NSFW channels or ones that are empty

            messageList.Enqueue(content); //add new message
			if (messageList.Count >= MAX_LOGGED_MESSAGES)
            {
                messageList.Dequeue(); //remove first value if there are too many of them
            }

            return Task.FromResult(0);
        }
    }
}