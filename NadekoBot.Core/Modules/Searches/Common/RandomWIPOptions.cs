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

        //Option to return an article that needs IRL info
        [Option('i', "irl", Required = false, Default = false, HelpText = "Return an article that needs IRL info")]
        public bool isIrl { get; set; } = false;

        //Option to return an article that needs appearance info
        [Option('a', "appearance", Required = false, Default = false, HelpText = "Return an article that needs appearance info")]
        public bool isAppearance { get; set; } = false;

        public void NormalizeOptions() { /*nothing to normalise*/ }
    }
}