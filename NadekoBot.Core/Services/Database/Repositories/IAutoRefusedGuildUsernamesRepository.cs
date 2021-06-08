using NadekoBot.Core.Common;
using System.Collections.Generic;

namespace NadekoBot.Core.Services.Database.Repositories
{
    public interface IAutoRefusedGuildUsernamesRepository
    {
        List<string> GetAllBannedWords(ulong guildId);
        BooleanServiceResult AddBannedWord(ulong guildId, string word);
        BooleanServiceResult RemoveBannedWord(ulong guildId, string word);
        void RemoveAllBannedWords(ulong guildId);
    }
}
