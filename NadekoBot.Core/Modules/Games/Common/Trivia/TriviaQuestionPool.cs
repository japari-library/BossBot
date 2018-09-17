using System.Collections.Generic;
using NadekoBot.Common;
using NadekoBot.Extensions;
using NadekoBot.Core.Services;
using NadekoBot.Core.Common.Pokemon;

namespace NadekoBot.Modules.Games.Common.Trivia
{
    public class TriviaQuestionPool
    {
        private readonly IDataCache _cache;
        private readonly int maxPokemonId;
		//ZGD: variable used for Friend list count
		private readonly int maxFriendId;

        private readonly NadekoRandom _rng = new NadekoRandom();

        private TriviaQuestion[] Pool => _cache.LocalData.TriviaQuestions;
        private IReadOnlyDictionary<int, string> Map => _cache.LocalData.PokemonMap;
		
		//ZGD: Dictionary data for Friend List and initialize Friend
		private IReadOnlyDictionary<int, FriendsNameId> Map2 => _cache.LocalData.FriendMap;		
		private FriendsNameId currentFriend;

        public TriviaQuestionPool(IDataCache cache)
        {
            _cache = cache;
            maxPokemonId = 721; //xd
			
			//ZGD: Initialize Friend List
			maxFriendId = Map2.Count;			
			FriendsNameId currentFriend = new FriendsNameId();
		}
		
		//ZGD: Commented original function parameters
        //public TriviaQuestion GetRandomQuestion(HashSet<TriviaQuestion> exclude, bool isPokemon)
		public TriviaQuestion GetRandomQuestion(HashSet<TriviaQuestion> exclude, bool isPokemon, bool isFriends, bool isFriendsHard)
        {
            if (Pool.Length == 0)
                return null;

            if (isPokemon)
            {
                var num = _rng.Next(1, maxPokemonId + 1);
                return new TriviaQuestion("Who's That Pokémon?", 
                    Map[num].ToTitleCase(),
                    "Pokemon",
                    $@"http://nadekobot.me/images/pokemon/shadows/{num}.png",
                    $@"http://nadekobot.me/images/pokemon/real/{num}.png");
            }
			
			//ZGD: Trivia Question trivia-friends block of code.
			if (isFriends || isFriendsHard)
			{
                var num = _rng.Next(0, maxFriendId);
				
				currentFriend = Map2[num];

				string friendName = currentFriend.Name;
				string friendTriviaid = currentFriend.Triviaid;
				
				string friendUnderscore = friendName.Replace(" ", "_");
				friendUnderscore = friendUnderscore.Replace("'", "_");
				
				if(isFriendsHard)
				{
					return new TriviaQuestion("Who's That Friend?!", 
						friendName.ToTitleCase(),
						"Friends Hard",
						$@"http://178.128.31.42/friends/SR/{friendTriviaid}.png",
						$@"http://178.128.31.42/friends/RR/{friendTriviaid}.png");
				}
				else
				{
					return new TriviaQuestion("Name That Friend!", 
						friendName.ToTitleCase(),
						"Friends",
						$@"http://178.128.31.42/friends/RR/{friendTriviaid}.png",
						$@"");
				}
			}
			//ZGD: end of trivia-friends question
			
            TriviaQuestion randomQuestion;
            while (exclude.Contains(randomQuestion = Pool[_rng.Next(0, Pool.Length)])) ;

            return randomQuestion;
        }
    }
}
