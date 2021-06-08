using NadekoBot.Core.Services.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using NadekoBot.Core.Common;

namespace NadekoBot.Core.Services.Database.Repositories.Impl
{
    public class AutoRefusedGuildUsernamesRepository : Repository<AutoRefusedGuildUsername>, IAutoRefusedGuildUsernamesRepository
    {
        public AutoRefusedGuildUsernamesRepository(DbContext context) : base(context)
        {
        }

        public BooleanServiceResult AddBannedWord(ulong guildId, string word)
        {
            if (string.IsNullOrEmpty(word) || _set.Any(x => x.GuildId == guildId && x.BannedText == word))
            {
                return BooleanServiceResult.False;
            }

            AutoRefusedGuildUsername newBannedWord = new AutoRefusedGuildUsername
            {
                GuildId = guildId,
                BannedText = word
            };
            _set.Add(newBannedWord);
            return BooleanServiceResult.True;
        }

        public BooleanServiceResult RemoveBannedWord(ulong guildId, string word)
        {
            AutoRefusedGuildUsername oldBannedWord = _set.FirstOrDefault(x => x.GuildId == guildId && x.BannedText == word);
            if (oldBannedWord == null)
            {
                return BooleanServiceResult.False;
            }

            _set.Remove(oldBannedWord);
            return BooleanServiceResult.True;
        }

        public List<string> GetAllBannedWords(ulong guildId)
        {
            return _set
                .Where(x => x.GuildId == guildId)
                .Select(x => x.BannedText)
                .ToList();
        }

        public void RemoveAllBannedWords(ulong guildId)
        {
            List<AutoRefusedGuildUsername> wordsToRemove = _set.Where(x => x.GuildId == guildId).ToList();
            if (!wordsToRemove.Any())
            {
                return;
            }

            _set.RemoveRange(wordsToRemove);
        }
    }
}
