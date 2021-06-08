using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using NadekoBot.Core.Services;
using NLog;
using Discord;
using System.Collections.Generic;
using NadekoBot.Extensions;
using NadekoBot.Core.Common;

namespace NadekoBot.Modules.Administration.Services
{
    public class AutoRefusedUsernamesService : INService
    {
        private readonly Logger _log;
        private readonly DiscordSocketClient _client;
        private readonly DbService _db;
        private readonly IBotCredentials _creds;

        public AutoRefusedUsernamesService(DiscordSocketClient client, NadekoBot bot, DbService db, IBotCredentials creds)
        {
            _log = LogManager.GetCurrentClassLogger();
            _client = client;
            _db = db;
            _creds = creds;

            _client.UserJoined += CheckUserForBadUsername;
        }

        private Task CheckUserForBadUsername(IGuildUser user)
        {
            var _ = Task.Run(async () =>
            {
                try
                {
                    if (!IsUsernameValid(user.GuildId, user.Nickname))
                    {
                        await HandleInvalidNickname(user);
                    }
                }
                catch (Exception ex) { _log.Warn(ex); }
            });
            return Task.CompletedTask;
        }

        private bool IsUsernameValid(ulong guildId, string nickname)
        {
            using (var uow = _db.UnitOfWork)
            {
                if (!uow.AutoRefusedGuildUsernameTogglesRepository.GetToggleStatus(guildId))
                {
                    return true;
                }
                List<string> bannedWords = uow.AutoRefusedGuildUsernamesRepository.GetAllBannedWords(guildId);

                return !bannedWords.Any(x => nickname.Contains(x));
            }
        }

        private async Task HandleInvalidNickname(IGuildUser user)
        {
            // We can't stop Dyno from adding the role originally, so we want to wait a bit for the role to be added
            // before taking it away.
            await Task.Delay(3000);

            List<IRole> currentUserRoles = user.GetRoles().ToList();
            if (currentUserRoles.Any())
            {
                await user.RemoveRolesAsync(user.GetRoles());
            }

            ulong roleId = ulong.Parse(_creds.InvalidUsernameRoleId);
            await user.AddRoleAsync(user.Guild.Roles.First(x => x.Id == roleId));
        }

        public Task<BooleanServiceResult> ToggleNamePolicy(ulong guildId)
        {
            BooleanServiceResult result = BooleanServiceResult.Failure;
            using (var uow = _db.UnitOfWork)
            {
                try
                {
                    result = uow.AutoRefusedGuildUsernameTogglesRepository.ToggleAutoRefuse(guildId)
                        ? BooleanServiceResult.True
                        : BooleanServiceResult.False;
                }
                catch (Exception ex) { _log.Warn(ex.Message); }
            }
            return Task.FromResult(result);
        }

        public Task<BooleanServiceResult> AddNameFilter(ulong guildId, string word)
        {
            BooleanServiceResult added = BooleanServiceResult.Failure;
            using (var uow = _db.UnitOfWork)
            {
                try
                {
                    added = uow.AutoRefusedGuildUsernamesRepository.AddBannedWord(guildId, word);
                }
                catch (Exception ex) { _log.Warn(ex.Message); }
            }
            return Task.FromResult(added);
        }

        public Task<BooleanServiceResult> RemoveNameFilter(ulong guildId, string word)
        {
            BooleanServiceResult found = BooleanServiceResult.Failure;
            using (var uow = _db.UnitOfWork)
            {
                try
                {
                    found = uow.AutoRefusedGuildUsernamesRepository.RemoveBannedWord(guildId, word);
                }
                catch (Exception ex) { _log.Warn(ex.Message); }
            }
            return Task.FromResult(found);
        }

        public Task<SimpleServiceResult> RemoveAllNameFilters(ulong guildId)
        {
            SimpleServiceResult result = SimpleServiceResult.Failure;
            using (var uow = _db.UnitOfWork)
            {
                try
                {
                    uow.AutoRefusedGuildUsernamesRepository.RemoveAllBannedWords(guildId);
                    result = SimpleServiceResult.Success;
                }
                catch (Exception ex) { _log.Warn(ex.Message); }
            }
            return Task.FromResult(result);
        }

        public Task<List<string>> ListNameFilters(ulong guildId)
        {
            List<string> result = null;
            using (var uow = _db.UnitOfWork)
            {
                try
                {
                    result = uow.AutoRefusedGuildUsernamesRepository.GetAllBannedWords(guildId);
                }
                catch (Exception ex) { _log.Warn(ex.Message); }
            }
            return Task.FromResult(result);
        }
    }
}
