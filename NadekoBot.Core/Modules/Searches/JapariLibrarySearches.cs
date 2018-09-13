using Discord;
using Discord.Commands;
using NadekoBot.Extensions;
using NadekoBot.Modules.Searches.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NadekoBot.Common.Attributes;
using NadekoBot.Core.Common.Pokemon;
using NadekoBot.Core.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System;
using NadekoBot.Core.Modules.Searches.Common;

namespace NadekoBot.Modules.Searches
{
    public partial class Searches
    {
        public class JapariLibrarySearchCommand : NadekoSubmodule<SearchesService>
        {
            private readonly IHttpClientFactory _httpFactory;
            public JapariLibrarySearchCommand(IHttpClientFactory factory)
            {
                _httpFactory = factory;
            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task JapariWiki([Remainder] string query = null)
            {
                query = query?.Trim();
                if (string.IsNullOrWhiteSpace(query))
                {
                    await ReplyErrorLocalized("jl_wikisearch_query_empty").ConfigureAwait(false); return;
                }
                var msg = await Context.Channel.SendMessageAsync(GetText("jl_wikisearch_searching"));
                string json;
                using (var http = _httpFactory.CreateClient())
                {
                    try
                    {
                        json = await http.GetStringAsync(String.Format("https://japari-library.com/w/api.php?action=query&formatversion=2&format=json&generator=search&gsrsearch={0}&gsrlimit=1&prop=info|revisions&inprop=url", Uri.EscapeDataString(query)));
                    }
                    catch (System.Net.Http.HttpRequestException)
                    {
                        await msg.ModifyAsync(m => m.Content = GetText("jl_wikisearch_error"));
                        return;
                    }
                }
                
                var data = JsonConvert.DeserializeObject<JapariLibraryAPIModel>(json);
                if (data.query == null)
                {
                    await msg.ModifyAsync(m => m.Content = GetText("jl_wikisearch_article_not_found"));
                    return;
                }
                await msg.ModifyAsync(m => m.Content = GetText("jl_wikisearch_success", query, data.query.pages[0].touched, data.query.pages[0].revisions[0].user, data.query.pages[0].lastrevid, data.query.pages[0].fullurl));
            }
        }
    }
}
