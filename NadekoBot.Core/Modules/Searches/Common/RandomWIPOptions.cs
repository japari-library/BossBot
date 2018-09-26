using CommandLine;
using NadekoBot.Core.Common;

namespace NadekoBot.Core.Modules.Searches.Common
{
    public class RandomWIPOptions : INadekoCommandOptions
    {
        //Option to make search results unembedded
        [Option('n', "unembed", Required = false, Default = false, HelpText = "Unembed the link")]
        public bool IsUnembedded { get; set; } = false;

        //Option to return a priority article
        [Option('p', "priority", Required = false, Default = false, HelpText = "Return a Priority Article")]
        public bool isPriority { get; set; } = false;

        public void NormalizeOptions() { /*nothing to normalise*/ }
    }
}