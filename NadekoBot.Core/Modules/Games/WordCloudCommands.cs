using Discord.Commands;
using NadekoBot.Common.Attributes;
using NadekoBot.Extensions;
using NadekoBot.Modules.Games.Services;
using NadekoBot.Core.Services;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NadekoBot.Modules.Games
{
    public partial class Games
    {
        [Group]
        public class WordCloudCommands : NadekoSubmodule<GamesService>
        {
            private readonly IMessageLoggerService _messageLoggerService;

            public WordCloudCommands(IMessageLoggerService messageLoggerService)
            {
                _messageLoggerService = messageLoggerService;
            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task wordcloud()
            {
                Queue<string> messages = _messageLoggerService.MessageList;

                string messagesString = "";
                foreach (string s in messages)
                {
                    messagesString += s + " ";
                }

                var msg = await Context.Channel.SendMessageAsync(messagesString).ConfigureAwait(false);
            }
        }
    }
}