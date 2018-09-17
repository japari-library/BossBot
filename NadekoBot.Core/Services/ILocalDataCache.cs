using NadekoBot.Core.Common.Pokemon;
using NadekoBot.Modules.Games.Common.Trivia;
using System.Collections.Generic;

namespace NadekoBot.Core.Services
{
    public interface ILocalDataCache
    {
        IReadOnlyDictionary<string, SearchPokemon> Pokemons { get; }
        IReadOnlyDictionary<string, SearchPokemonAbility> PokemonAbilities { get; }
        TriviaQuestion[] TriviaQuestions { get; }
        IReadOnlyDictionary<int, string> PokemonMap { get; }
		
		//ZGD: interface of FriendMap, used in trivia-friends functionality.
		//ZGD: be sure to check RedisLocalDataCache.cs as well.
		IReadOnlyDictionary<int, FriendsNameId> FriendMap { get; }
    }
}
