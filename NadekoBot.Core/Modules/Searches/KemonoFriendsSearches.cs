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
using System.Text.RegularExpressions;
using NadekoBot.Core.Common; //for OptionParser
using Configuration = AngleSharp.Configuration; //for parsing html page
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using AngleSharp.Parser.Html;

namespace NadekoBot.Modules.Searches
{
    public partial class Searches
    {
        public class KemonoFriendsSearchCommand : NadekoSubmodule<SearchesService>
        {
            private readonly IHttpClientFactory _httpFactory;
            public KemonoFriendsSearchCommand(IHttpClientFactory factory)
            {
                _httpFactory = factory;
            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task JapariWiki([Remainder] string query = null)
            {
                query = query?.Trim();

                // reply to the user if the query is empty or over 1024 characters
                if (string.IsNullOrWhiteSpace(query))
                {
                    await ReplyErrorLocalized("kf_wikisearch_query_empty").ConfigureAwait(false); return;
                }
                else if (query.Length > 1024)
                {
                    await ReplyErrorLocalized("kf_wikisearch_query_too_long").ConfigureAwait(false); return;
                }

                var msg = await Context.Channel.SendMessageAsync(GetText("kf_wikisearch_searching")).ConfigureAwait(false);
                // search with the query
                using (var http = _httpFactory.CreateClient())
                {
                    try
                    {
                        // search with the query
                        string queryApiUrl = "https://japari-library.com/w/api.php?action=query&formatversion=2&format=json&generator=search&gsrsearch={0}&gsrlimit=1&prop=info|revisions&inprop=url&redirects";
                        string queryJson = await http.GetStringAsync(String.Format(queryApiUrl, Uri.EscapeDataString(query))).ConfigureAwait(false);
                        var data = JsonConvert.DeserializeObject<JapariLibraryQueryAPIModel>(queryJson);
                        // reply to the user if their query cannot be found
                        if (data.query == null)
                        {
                            await msg.ModifyAsync(m => m.Content = GetText("kf_wikisearch_article_not_found")).ConfigureAwait(false);
                            return;
                        }
                        var queryPage = data.query.pages[0];
                        // get page information
                        string parseApiUrl = "https://japari-library.com//w/api.php?action=parse&format=json&page={0}&prop=categories%7Cimages%7Cdisplaytitle%7Cproperties&formatversion=latest&redirects";
                        string parseJson = await http.GetStringAsync(String.Format(parseApiUrl, Uri.EscapeDataString(data.query.pages[0].title))).ConfigureAwait(false);
                        var parseData = JsonConvert.DeserializeObject<JapariLibraryParseAPIModel>(parseJson);
                        if (parseData.parse == null)
                        {
                            await msg.ModifyAsync(m => m.Content = GetText("kf_wikisearch_article_not_found")).ConfigureAwait(false);
                            return;
                        }
                        var parsePage = parseData.parse;
                        var imageUrl = "";
                        // get image because mediawiki doesn't want to expose image URL

                        var config = Configuration.Default.WithDefaultLoader();

                        using (var document = await BrowsingContext.New(config).OpenAsync(queryPage.fullurl).ConfigureAwait(false)) //get pictures by going through the friend page
                        {
                            var imageElem = document.QuerySelector("table.infobox > tbody > tr >td > p > a.image > img");
                            imageElem = ((IHtmlImageElement)imageElem)?.Source == null ? document.QuerySelector("div.tabbertab > p > a > img") : imageElem; //if a friend page has multiple Friend pictures, this will get the corrct image
                            imageUrl = ((IHtmlImageElement)imageElem)?.Source ?? "http://icecream.me/uploads/870b03f36b59cc16ebfe314ef2dde781.png"; //get friend image or a default one if one cannot be loaded
                        }

                        await msg.DeleteAsync().ConfigureAwait(false);
                        // footer 
                        var footer = new EmbedFooterBuilder();
                        footer.Text = "Japari Library";
                        footer.IconUrl = "https://japari-library.com/w/resources/assets/Jlibrarywglogo.png?d63ab";
                        // truncated version of the query (max 50 characters + ellipsis) to be shown back
                        string truncatedQuery = query.Length <= 50 ? query : query.Substring(0, 50) + "...";
                        // send with embed 
                        await Context.Channel.EmbedAsync(new EmbedBuilder()
                        .WithFooter(footer)
                        .WithColor(new Discord.Color(52, 152, 219))
                        .WithTitle(parsePage.title)
                        .WithDescription(parsePage.properties.description)
                        .WithThumbnailUrl(imageUrl)
                        .WithUrl(queryPage.fullurl)
                        .AddField("Search Term", truncatedQuery, inline: true)
                        .AddField("Revision", queryPage.revisions[0].timestamp, inline: true)
                        .AddField("Revision By", queryPage.revisions[0].user, inline: true)
                        .AddField("Revision ID", queryPage.revisions[0].revid, inline: true)
                        );
                    }
                    catch (System.Net.Http.HttpRequestException)
                    {
                        await msg.ModifyAsync(m => m.Content = GetText("kf_wikisearch_error")).ConfigureAwait(false);
                        return;
                    }
                }
            }

            [NadekoCommand, Usage, Description, Aliases]
            [NadekoOptions(typeof(FriendSearchOptions))]
            public async Task RandomFriend(params string[] args)
            {
                var (opts, _) = OptionsParser.ParseFrom(new FriendSearchOptions(), args); //parse options, to be used to check if the result should be unembedded

                var msg = await Context.Channel.SendMessageAsync(GetText("kf_randomfriend_searching")).ConfigureAwait(false);
                string friendPage; //we need to declare this here to use it out of the do-while loop
                using (var http = _httpFactory.CreateClient())
                {
                    do
                    {
                        try
                        {
                            var response = await http.GetAsync("https://japari-library.com/wiki/Special:RandomInCategory/Friends").ConfigureAwait(false);
                            var location = response.Headers.Location; //the redirect isn't automatically followed, you have to dig in the response header to find out what the page actually is
                            friendPage = location.AbsoluteUri;
                        }
                        catch (System.Net.Http.HttpRequestException)
                        {
                            await msg.ModifyAsync(m => m.Content = GetText("kf_wikisearch_error")).ConfigureAwait(false);
                            return;
                        }
                        friendPage = Regex.Replace(friendPage, "http*", "https"); //the URI is http by default while japari-librari is https, the vanilla link works but this is more correct

                    } while (friendPage.Contains("Category:")); //Category pages count as Friend pages but we don't want none of that

                    if (opts.IsUnembedded)
                    {
                        friendPage = "<" + friendPage + ">"; //Enclosing a link in these tells Discord not to make an embed for it
                        await msg.ModifyAsync(m => m.Content = GetText("kf_randomfriend_success", friendPage)).ConfigureAwait(false);
                    }
                    else //make it embedded
                    {

                        var config = Configuration.Default.WithDefaultLoader();

                        using (var document = await BrowsingContext.New(config).OpenAsync(friendPage).ConfigureAwait(false))
                        {
                            var imageElem = document.QuerySelector("table.infobox > tbody > tr >td > p > a.image > img");
                            imageElem = ((IHtmlImageElement)imageElem)?.Source == null ? document.QuerySelector("div.tabbertab > p > a > img") : imageElem; //if a friend page has multiple Friend pictures, this will get the corrct image
                            var friendImageUrl = ((IHtmlImageElement)imageElem)?.Source ?? "http://icecream.me/uploads/870b03f36b59cc16ebfe314ef2dde781.png"; //get friend image or a default one if one cannot be loaded

                            var friendInfoElem = document.QuerySelector("#mw-content-text > p");
                            var friendInfo = friendInfoElem == null ? "Description unavailable" : friendInfoElem.InnerHtml;

                            var friendNameElem = document.QuerySelector("#firstHeading");
                            var friendName = friendNameElem.InnerHtml; //get friend name

                            friendName = Regex.Replace(friendName, "<[^>]*>", "");
                            friendInfo = Regex.Replace(friendInfo, "<[^>]*>", ""); //remove html tags

                            // footer 
                            var footer = new EmbedFooterBuilder();
                            footer.Text = "Japari Library";
                            footer.IconUrl = "https://japari-library.com/w/resources/assets/Jlibrarywglogo.png?d63ab";

                            await msg.DeleteAsync();
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithOkColor() //make a small embed
                            .WithTitle(friendName)
                            .WithDescription(friendInfo)
                            .WithThumbnailUrl(friendImageUrl)
                            .WithUrl(friendPage)
                            .WithFooter(footer)
                            ).ConfigureAwait(false);
                        }
                    }
                }
            }

            [NadekoCommand, Usage, Description, Aliases]
            [NadekoOptions(typeof(RandomWIPOptions))]
            public async Task WIP(params string[] args)
            {
                var (opts, _) = OptionsParser.ParseFrom(new RandomWIPOptions(), args); //parse options, to be used to check if the result should be unembedded

                var msg = await Context.Channel.SendMessageAsync(GetText("kf_randomfriend_searching")).ConfigureAwait(false);
                string wipPage; //we need to declare this here to use it out of the do-while loop

                string redirPage = "https://japari-library.com/wiki/Special:RandomInCategory/Missing_Content"; //by default look in the Missing_Content category
                //if requested, look for a specific kind of WIP page (unfortunately only 1 of these is possible at a time)
                if (opts.isIrl)
                {
                    redirPage = "https://japari-library.com/wiki/Special:RandomInCategory/Needs_RL_Info";
                }
                if (opts.isAppearance)
                {
                    redirPage = "https://japari-library.com/wiki/Special:RandomInCategory/Needs_Appearance";
                }
                if (opts.isPriority)
                {
                    redirPage = "https://japari-library.com/wiki/Special:RandomInCategory/Priority_Articles";
                }

                using (var http = _httpFactory.CreateClient())
                {
                    do
                    {
                        try
                        {
                            var response = await http.GetAsync(redirPage).ConfigureAwait(false);
                            var location = response.Headers.Location; //the redirect isn't automatically followed, you have to dig in the response header to find out what the page actually is
                            wipPage = location.AbsoluteUri;
                        }
                        catch (System.Net.Http.HttpRequestException)
                        {
                            await msg.ModifyAsync(m => m.Content = GetText("kf_wikisearch_error")).ConfigureAwait(false);
                            return;
                        }
                        wipPage = Regex.Replace(wipPage, "http*", "https"); //the URI is http by default while japari-librari is https, the vanilla link works but this is more correct

                    } while (wipPage.Contains("Category:")); //Category pages count as Friend pages but we don't want none of that

                    if (opts.IsUnembedded)
                    {
                        wipPage = "<" + wipPage + ">"; //Enclosing a link in these tells Discord not to make an embed for it

                        string successText = "kf_randomwip_success";
                        if (opts.isPriority)
                        {
                            successText = "kf_randomwip_priority_success";
                        }
                        await msg.ModifyAsync(m => m.Content = GetText(successText, wipPage)).ConfigureAwait(false);
                    }
                    else //make it embedded
                    {

                        var config = Configuration.Default.WithDefaultLoader();

                        using (var document = await BrowsingContext.New(config).OpenAsync(wipPage).ConfigureAwait(false))
                        {
                            var imageElem = document.QuerySelector("table.infobox > tbody > tr >td > p > a.image > img");
                            imageElem = ((IHtmlImageElement)imageElem)?.Source == null ? document.QuerySelector("div.tabbertab > p > a > img") : imageElem; //if a wip page has multiple pictures, this will get the correct image
                            var friendImageUrl = ((IHtmlImageElement)imageElem)?.Source ?? "http://icecream.me/uploads/870b03f36b59cc16ebfe314ef2dde781.png"; //get wip image or a default one if one cannot be loaded

                            var friendInfoElem = document.QuerySelector("#mw-content-text > p");
                            // check if we can get the description or not, if not, just say the description is unavailable
                            var friendInfo = friendInfoElem == null ? "Description unavailable" : friendInfoElem.InnerHtml;

                            var friendNameElem = document.QuerySelector("#firstHeading");
                            var friendName = friendNameElem.InnerHtml; //get page name

                            friendName = Regex.Replace(friendName, "<[^>]*>", "");
                            friendInfo = Regex.Replace(friendInfo, "<[^>]*>", ""); //remove html tags

                            // footer 
                            var footer = new EmbedFooterBuilder();
                            footer.Text = "Japari Library";
                            footer.IconUrl = "https://japari-library.com/w/resources/assets/Jlibrarywglogo.png?d63ab";

                            await msg.DeleteAsync();
                            await Context.Channel.EmbedAsync(new EmbedBuilder().WithOkColor() //make a small embed
                            .WithTitle(friendName)
                            .WithDescription(friendInfo)
                            .WithThumbnailUrl(friendImageUrl)
                            .WithUrl(wipPage)
                            .WithFooter(footer)
                            ).ConfigureAwait(false);
                        }
                    }
                }
            }

            [NadekoCommand, Usage, Description, Aliases]
            public async Task RateUp([Remainder] string query = null)
            {
                query = query?.Trim();

                // reply to the user if the query is empty or over 1024 characters
                if (string.IsNullOrWhiteSpace(query))
                {
                    await ReplyErrorLocalized("kf_rateup_query_empty").ConfigureAwait(false); return;
                }
                else if (query.Length > 1024)
                {
                    await ReplyErrorLocalized("kf_wikisearch_query_too_long").ConfigureAwait(false); return;
                }

                Console.WriteLine(query);
                // find friend and area (if given) name from query
                string[] arguments = query.Split(' ');
                for (int i = 0; i < arguments.Length; i++)
                {
                    arguments[i] = Uri.EscapeDataString(arguments[i]); // escape all arguments
                }
                string friendName = arguments[0];
                string areaName = arguments.Length > 1 ? arguments[1] : null;
                Console.WriteLine(arguments);

                var msg = await Context.Channel.SendMessageAsync(GetText("kf_wikisearch_searching")).ConfigureAwait(false);
                // search with the query
                using (var http = _httpFactory.CreateClient())
                {
                    try
                    {
                        string partialQuery = areaName != null ? "friend={0}&area={1}" : "friend={0}";
                        // search with the query
                        string queryApiUrl = "http://www.smidgeindustriesltd.com/kf/dropdata/botdata.php?" + partialQuery;
                        // String.Format will ignore the areaName if it isn't needed
                        string queryResponse = await http.GetStringAsync(String.Format(queryApiUrl, friendName, areaName)).ConfigureAwait(false);
                        Console.WriteLine(String.Format(queryApiUrl, friendName, areaName));

                        Queue<string> splitQueryResponse = new Queue<string>();
                        for (int i = 0; i < queryResponse.Length; i += 2000)
                        {
                            splitQueryResponse.Enqueue(queryResponse.Substring(i, Math.Min(queryResponse.Length - i, 2000)));
                        }

                        await msg.ModifyAsync(m => m.Content = GetText("simple", splitQueryResponse.Dequeue())).ConfigureAwait(false);
                        while (splitQueryResponse.Count != 0)
                        {
                            await Context.Channel.SendMessageAsync(GetText("simple", splitQueryResponse.Dequeue())).ConfigureAwait(false);
                        }
                    }
                    catch (System.Net.Http.HttpRequestException)
                    {
                        await msg.ModifyAsync(m => m.Content = GetText("kf_rateup_commmunication_error")).ConfigureAwait(false);
                        return;
                    }
                }
            }
        }
    }
}
