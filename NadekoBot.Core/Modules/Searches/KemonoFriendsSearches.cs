using Discord;
using Discord.Commands;
using NadekoBot.Extensions;
using NadekoBot.Modules.Searches.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NadekoBot.Common.Attributes;
using Newtonsoft.Json;
using System.Net.Http;
using System;
using NadekoBot.Core.Modules.Searches.Common;
using System.Text.RegularExpressions;
using NadekoBot.Core.Common; //for OptionParser
using Configuration = AngleSharp.Configuration; //for parsing html page
using AngleSharp;
using NadekoBot.Core.Services;
using NadekoBot.Core.Common.Pokemon;
using System.Net;
using AngleSharp.Html.Dom;
using AngleSharp.Dom;

namespace NadekoBot.Modules.Searches
{
    public partial class Searches
    {
        public class KemonoFriendsSearchCommand : NadekoSubmodule<SearchesService>
        {
            private readonly IHttpClientFactory _httpFactory;
            private readonly IDataCache _cache;
            private readonly IBotCredentials _creds;
            public KemonoFriendsSearchCommand(IHttpClientFactory factory, IDataCache cache, IBotCredentials creds)
            {
                _httpFactory = factory;
                _cache = cache;
                _creds = creds;
            }

            private IReadOnlyDictionary<int, FriendsNameId> friendImageDbMap => _cache.LocalData.FriendMap;

            [NadekoCommand, Usage, Description, Aliases]
            public async Task JapariWiki([Remainder] string query = null)
            {
                query = query?.Trim();

                // reply to the user if the query is empty or over 1024 characters
                if (string.IsNullOrWhiteSpace(query))
                {
                    await ReplyErrorLocalizedAsync("kf_wikisearch_query_empty").ConfigureAwait(false); return;
                }
                else if (query.Length > 1024)
                {
                    await ReplyErrorLocalizedAsync("kf_wikisearch_query_too_long").ConfigureAwait(false); return;
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
                        string imageUrl = string.Empty;
                        string friendName = string.Empty;
                        // get image because mediawiki doesn't want to expose image URL

                        var config = Configuration.Default.WithDefaultLoader();

                        using (var document = await BrowsingContext.New(config).OpenAsync(queryPage.fullurl).ConfigureAwait(false)) //get pictures by going through the friend page
                        {
                            imageUrl = (document.QuerySelector(".image > img") as IHtmlImageElement)?.Source;
                            friendName = (document.QuerySelector(".firstHeading") as IHtmlHeadingElement).GetInnerText();
                        }

                        // ! Possible bug in the old version of .NET Core used to build Boss.
                        // A couple of image Urls are not recognised as valid Uris.
                        // We can hack these few cases in by matching the friend name (found in the webpage) with the trivia image database.
                        // Ideally, we want to upgrade the bot to use a higher version of Net Core so this hack isn't needed.
                        if (!Uri.IsWellFormedUriString(imageUrl, UriKind.RelativeOrAbsolute))
                        {
                            string friendId = friendImageDbMap.Values.FirstOrDefault(x => x.Name == friendName)?.Triviaid;
                            imageUrl = !string.IsNullOrEmpty(friendId)
                                ? $@"{_creds.KFTriviaBaseURL}/RR/{friendId}.png"
                                : "https://japari-library.com/w/resources/assets/Jlibrarywglogo.png?d63ab";
                        }

                        await msg.DeleteAsync().ConfigureAwait(false);
                        // footer 
                        var footer = new EmbedFooterBuilder();
                        footer.Text = "Japari Library";
                        footer.IconUrl = "https://japari-library.com/w/resources/assets/Jlibrarywglogo.png?d63ab";
                        // truncated version of the query (max 50 characters + ellipsis) to be shown back
                        string truncatedQuery = query.Length <= 50 ? query : query.Substring(0, 50) + "...";
                        // send with embed
                        EmbedBuilder embedBuilder = new EmbedBuilder()
                        .WithFooter(footer)
                        .WithColor(new Discord.Color(52, 152, 219))
                        .WithTitle(parsePage.title)
                        .WithThumbnailUrl(imageUrl)
                        .AddField("Search Term", truncatedQuery, inline: true)
                        .AddField("Revision", queryPage.revisions[0].timestamp, inline: true)
                        .AddField("Revision By", queryPage.revisions[0].user, inline: true)
                        .AddField("Revision ID", queryPage.revisions[0].revid, inline: true);

                        // Same error as for the image Url, where the Url is not recognised as valid.
                        // Best we can do for now is displaying the Url separately.
                        if (Uri.IsWellFormedUriString(queryPage.fullurl, UriKind.RelativeOrAbsolute))
                        {
                            embedBuilder
                                .WithUrl(queryPage.fullurl)
                                .WithDescription(WebUtility.HtmlDecode(parsePage.properties.description));
                        }
                        else
                        {
                            // xd
                            embedBuilder.WithDescription(queryPage.fullurl + Environment.NewLine + WebUtility.HtmlDecode(parsePage.properties.description));
                        }

                        await Context.Channel.EmbedAsync(embedBuilder);
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

                        string friendImageUrl;
                        string friendName;
                        string friendInfo;

                        using (var document = await BrowsingContext.New(config).OpenAsync(friendPage).ConfigureAwait(false))
                        {
                            friendImageUrl = (document.QuerySelector(".image > img") as IHtmlImageElement)?.Source;
                            friendName = (document.QuerySelector(".firstHeading") as IHtmlHeadingElement)?.GetInnerText() ?? "Name Unavailable";
                            friendInfo = (document.QuerySelector("#mw-content-text > p") as IHtmlParagraphElement)?.GetInnerText() ?? "Description Unavailable";
                        }

                        // ! Possible bug in the old version of .NET Core used to build Boss.
                        // A couple of image Urls are not recognised as valid Uris.
                        // We can hack these few cases in by matching the friend name (found in the webpage) with the trivia image database.
                        // Ideally, we want to upgrade the bot to use a higher version of Net Core so this hack isn't needed.
                        if (friendImageUrl == null || !Uri.IsWellFormedUriString(friendImageUrl, UriKind.RelativeOrAbsolute))
                        {
                            string friendId = friendImageDbMap.Values.FirstOrDefault(x => x.Name == friendName)?.Triviaid;
                            friendImageUrl = !string.IsNullOrEmpty(friendId)
                                ? $@"{_creds.KFTriviaBaseURL}/RR/{friendId}.png"
                                : "https://japari-library.com/w/resources/assets/Jlibrarywglogo.png?d63ab";
                        }
                        friendInfo = Regex.Replace(friendInfo, "<[^>]*>", ""); //remove html tags

                        // footer 
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                .WithText("Japari Library")
                                .WithIconUrl("https://japari-library.com/w/resources/assets/Jlibrarywglogo.png?d63ab");

                        await msg.DeleteAsync();

                        EmbedBuilder embedBuilder = new EmbedBuilder()
                            .WithFooter(footer)
                            .WithColor(new Discord.Color(52, 152, 219))
                            .WithTitle(friendName)
                            .WithThumbnailUrl(friendImageUrl);

                        // Same error as for the image Url, where the Url is not recognised as valid.
                        // Best we can do for now is displaying the Url separately.
                        if (Uri.IsWellFormedUriString(friendPage, UriKind.RelativeOrAbsolute))
                        {
                            embedBuilder
                                .WithUrl(friendPage)
                                .WithDescription(WebUtility.HtmlDecode(friendInfo));
                        }
                        else
                        {
                            // xd
                            embedBuilder.WithDescription(friendPage + Environment.NewLine + WebUtility.HtmlDecode(friendInfo));
                        }

                        await Context.Channel.EmbedAsync(embedBuilder).ConfigureAwait(false);
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

                        string friendImageUrl;
                        string friendName;
                        string friendInfo;

                        using (var document = await BrowsingContext.New(config).OpenAsync(wipPage).ConfigureAwait(false))
                        {
                            friendImageUrl = (document.QuerySelector(".image > img") as IHtmlImageElement)?.Source;
                            friendName = (document.QuerySelector(".firstHeading") as IHtmlHeadingElement)?.GetInnerText() ?? "Name Unavailable";
                            friendInfo = (document.QuerySelector("#mw-content-text > p") as IHtmlParagraphElement)?.InnerHtml ?? "Description Unavailable";
                        }

                        // ! Possible bug in the old version of .NET Core used to build Boss.
                        // A couple of image Urls are not recognised as valid Uris.
                        // We can hack these few cases in by matching the friend name (found in the webpage) with the trivia image database.
                        // Ideally, we want to upgrade the bot to use a higher version of Net Core so this hack isn't needed.
                        if (friendImageUrl == null || !Uri.IsWellFormedUriString(friendImageUrl, UriKind.RelativeOrAbsolute))
                        {
                            string friendId = friendImageDbMap.Values.FirstOrDefault(x => x.Name == friendName)?.Triviaid;
                            friendImageUrl = !string.IsNullOrEmpty(friendId)
                                ? $@"{_creds.KFTriviaBaseURL}/RR/{friendId}.png"
                                : "https://japari-library.com/w/resources/assets/Jlibrarywglogo.png?d63ab";
                        }
                        friendInfo = Regex.Replace(friendInfo, "<[^>]*>", ""); //remove html tags

                        // footer 
                        EmbedFooterBuilder footer = new EmbedFooterBuilder()
                                .WithText("Japari Library")
                                .WithIconUrl("https://japari-library.com/w/resources/assets/Jlibrarywglogo.png?d63ab");

                        await msg.DeleteAsync();

                        EmbedBuilder embedBuilder = new EmbedBuilder()
                            .WithFooter(footer)
                            .WithColor(new Discord.Color(52, 152, 219))
                            .WithTitle(friendName)
                            .WithThumbnailUrl(friendImageUrl);

                        // Same error as for the image Url, where the Url is not recognised as valid.
                        // Best we can do for now is displaying the Url separately.
                        if (Uri.IsWellFormedUriString(wipPage, UriKind.RelativeOrAbsolute))
                        {
                            embedBuilder
                                .WithUrl(wipPage)
                                .WithDescription(WebUtility.HtmlDecode(friendInfo));
                        }
                        else
                        {
                            // xd
                            embedBuilder.WithDescription(wipPage + Environment.NewLine + WebUtility.HtmlDecode(friendInfo));
                        }

                        await Context.Channel.EmbedAsync(embedBuilder).ConfigureAwait(false);
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
                    await ReplyErrorLocalizedAsync("kf_rateup_query_empty").ConfigureAwait(false);
                    return;
                }
                else if (query.Length > 64)
                {
                    await ReplyErrorLocalizedAsync("kf_rateup_query_too_long").ConfigureAwait(false);
                    return;
                }

                // find friend and area name (if given) from query
                IList<string> arguments = query.Split(" in ").Select(arg => Uri.EscapeDataString(arg)).ToList();
                string friendName = arguments[0];
                string areaName = arguments.Count > 1 ? arguments[1] : null;

                var msg = await Context.Channel.SendMessageAsync(GetText("kf_wikisearch_searching")).ConfigureAwait(false);

                using (var http = _httpFactory.CreateClient())
                {
                    try
                    {
                        string partialQuery = areaName != null ? "friend={0}&area={1}" : "friend={0}";
                        string queryApiUrl = "http://www.smidgeindustriesltd.com/kf/dropdata/botdata.php?" + partialQuery;
                        // String.Format will ignore the areaName if it isn't needed
                        string queryResponse = await http.GetStringAsync(string.Format(queryApiUrl, friendName, areaName)).ConfigureAwait(false);

                        string discordString = RateupQueryResult.ConvertToModel(queryResponse).GetDiscordString();

                        await msg.ModifyAsync(m => m.Content = GetText("simple", discordString)).ConfigureAwait(false);
                    }
                    catch (HttpRequestException)
                    {
                        await msg.ModifyAsync(m => m.Content = GetText("kf_rateup_commmunication_error")).ConfigureAwait(false);
                        return;
                    }
                }
            }
        }
    }
}
