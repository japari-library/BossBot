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
        
        private List<ulong> whitelistedChannelIDs = new List<ulong>() //specific channels to ignore, by ID
        {
            297640792590254080, //#rules
            375487069709139969, //#roles
            299847594035642368, //#announcements
            415277589976973314, //#events

            296745674752393216, //#japari-park
            301240213546729486, //#japari-cafe
            356990164167294976, //#japari-safari
            314680051729694720, //#japari-voice

            297873669341052928, //#kemofure-general
            300138414232305664, //#japari-library
            305077955972038656, //#japari-wiki
            405966834085527554, //#japari-pavillon
            454180339653279745, //#japari-festival
            
            297481522968133642, //#kemofure-art
            297481686134816770, //#kemofure-radio
            297862338864742401, //#kemofure-emotes

            414314121341829120, //#creative-index
            362822760960884756, //#japari-creative
            415278259685556224, //#creative-feedback
            414476864300515338, //#creative-storywriting

            301241261397114881, //#japari-akiba
            441700588061982730, //#japari-port
            321275668551958530, //#japari-arcade
            489295286414475275, //#japari-theatre
            482776248683331594, //#japari-gallery

            //for testing purposes
            489799195642036225, //#general
            490298280496988160, //#channel2
            490298423522885632 //#channel3 (going to be ignored anyway since it's NSFW tagged)

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
            content = Regex.Replace(content, "www*", " "); //remove URLs from message
            content = Regex.Replace(content, "http*", " ");
            content = Regex.Replace(content, "trpn*", " "); //remove trpn tags from message
            
            if (channel.IsNsfw) //ignore all NSFW channels
            {
                return Task.FromResult(0);
            }

            bool isWhitelistedChannel = false;
            foreach (ulong i in whitelistedChannelIDs) //accept messages from specific channels
            {
                if (channel.Id == i)
                {
                    isWhitelistedChannel = true;
                }
            }
            if (!isWhitelistedChannel || content == "") return Task.FromResult(0); //ignore empty messages as well as ones from ignored channels

            string[] words = content.Split(' ');
            
            foreach (string w in words)
            {
                if (w != "" && w[0] != '.' && w[0] != '-' && w[0] != '@')
                {
                    messageList.Enqueue(w); //add non-empty words, or those that are most likely meant to be parsed by bots
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