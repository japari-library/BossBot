using CommandLine;
using NadekoBot.Core.Common;

namespace NadekoBot.Core.Modules.Searches.Common
{
    public class SearchOptions : INadekoCommandOptions
    {
        //Option to make search results unembedded
        [Option('n', "unembed", Required = false, Default = false, HelpText = "Unembedded")]
        public bool IsUnembedded { get; set; } = false;
    }
}