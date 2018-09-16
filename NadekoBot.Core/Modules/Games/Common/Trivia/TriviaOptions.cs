﻿using CommandLine;
using NadekoBot.Core.Common;

namespace NadekoBot.Core.Modules.Games.Common.Trivia
{
    public class TriviaOptions : INadekoCommandOptions
    {
        [Option('p', "pokemon", Required = false, Default = false, HelpText = "Whether it's 'Who's that pokemon?' trivia.")]
        public bool IsPokemon { get; set; } = false;
        [Option("nohint", Required = false, Default = false, HelpText = "Don't show any hints.")]
        public bool NoHint { get; set; } = false;
        [Option('w', "win-req", Required = false, Default = 10, HelpText = "Winning requirement. Set 0 for an infinite game. Default 10.")]
        public int WinRequirement { get; set; } = 10;
        [Option('q', "question-timer", Required = false, Default = 30, HelpText = "How long until the question ends. Default 30.")]
        public int QuestionTimer { get; set; } = 30;
        [Option('t', "timeout", Required = false, Default = 0, HelpText = "Number of questions of inactivity in order stop. Set 0 for never. Default 10.")]
        public int Timeout { get; set; } = 10;
		
		//ZGD: Options added for the two trivia-friends categories.
		[Option('f', "friends", Required = false, Default = false, HelpText = "Name that Friend! ~ZGD")]
        public bool IsFriends { get; set; } = false;
		[Option('h', "friendshard", Required = false, Default = false, HelpText = "Name that Friend! Hard ~ZGD")]
        public bool IsFriendsHard { get; set; } = false;
		//ZGD: end of options

        //Option for leaving results unembedded
        [Option('n', "unembed", Required = false, Default = false, HelpText = "Unembedded")]
        public bool IsUnembedded { get; set } = false;

        public void NormalizeOptions()
        {
            if (WinRequirement < 0)
                WinRequirement = 10;
            if (QuestionTimer < 10 || QuestionTimer > 300)
                QuestionTimer = 30;
            if (Timeout < 0 || Timeout > 20)
                Timeout = 10;

        }
    }
}
