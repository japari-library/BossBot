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
        private readonly int MAX_LOGGED_WORDS = 5000; //technically not a magic number if i store it in a variable
        
        private List<ulong> ignoredChannelIDs = new List<ulong>() //specific channels to ignore, by ID
        {
            //insert ids here
        };

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
            
            bool isIgnoredChannel = false;
            if (channel.IsNsfw) //ignore all NSFW channels
            {
                isIgnoredChannel = true;
            }
            foreach (ulong i in ignoredChannelIDs) //ignore specific channels
            {
                if (channel.Id == i)
                {
                    isIgnoredChannel = true;
                }
            }

            if (isIgnoredChannel || content == "") return Task.FromResult(0); //ignore empty messages as well as ones from ignored channels

            string[] words = content.Split(' ');
            
            foreach (string w in words)
            {
                if (w != "" && w[0] != ('.' || '-'))
                {
                    messageList.Enqueue(w); //add non-empty words, or those that are most likely meant to be parsed by bots (starting with . or -)
                }
                
            }

			while (messageList.Count >= MAX_LOGGED_WORDS)
            {
                messageList.Dequeue(); //remove values at the beginning if there are too many of them
            }

            return Task.FromResult(0);
        }
    }
}