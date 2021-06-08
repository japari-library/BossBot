using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NadekoBot.Extensions;
using NadekoBot.Core.Services.Database.Models;
using System.Linq;
using System.Threading.Tasks;
using NadekoBot.Common.Attributes;
using NadekoBot.Modules.Administration.Services;
using NadekoBot.Core.Common.TypeReaders.Models;
using System;
using NadekoBot.Core.Common;
using System.Collections.Generic;

namespace NadekoBot.Modules.Administration
{
    public partial class Administration
    {
        [Group]
        public class AutoRefusedUsernamesCommands : NadekoSubmodule<AutoRefusedUsernamesService>
        {
            private readonly MuteService _mute;

            public AutoRefusedUsernamesCommands(MuteService mute)
            {
                _mute = mute;
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task ToggleNamePolicy()
            {
                // This bool? really should be an enum
                BooleanServiceResult result = await _service.ToggleNamePolicy(Context.Guild.Id);
                if (result == BooleanServiceResult.Failure)
                {
                    await ReplyErrorLocalized("command_failed").ConfigureAwait(false);
                }
                else
                {
                    await ReplyConfirmLocalized("username_policy_toggled", result).ConfigureAwait(false);
                }
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task AddNameFilter([Remainder] string word)
            {
                if (string.IsNullOrEmpty(word))
                {
                    await ReplyErrorLocalized("name_filter_must_not_be_empty").ConfigureAwait(false);
                }

                BooleanServiceResult added = await _service.AddNameFilter(Context.Guild.Id, word);
                if (added == BooleanServiceResult.Failure)
                {
                    await ReplyErrorLocalized("command_failed").ConfigureAwait(false);
                }
                else if (added == BooleanServiceResult.False)
                {
                    await ReplyErrorLocalized("name_filter_already_exists").ConfigureAwait(false);
                }
                else
                {
                    await ReplyConfirmLocalized("name_filter_added", added).ConfigureAwait(false);
                }
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task RemoveNameFilter([Remainder] string word)
            {
                if (string.IsNullOrEmpty(word))
                {
                    await ReplyErrorLocalized("name_filter_must_not_be_empty").ConfigureAwait(false);
                }

                BooleanServiceResult found = await _service.RemoveNameFilter(Context.Guild.Id, word);
                if (found == BooleanServiceResult.Failure)
                {
                    await ReplyErrorLocalized("command_failed").ConfigureAwait(false);
                }
                else if (found == BooleanServiceResult.False)
                {
                    await ReplyErrorLocalized("name_filter_not_found").ConfigureAwait(false);
                }
                else
                {
                    await ReplyConfirmLocalized("name_filter_removed").ConfigureAwait(false);
                }
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task RemoveAllNameFilters()
            {
                SimpleServiceResult found = await _service.RemoveAllNameFilters(Context.Guild.Id);
                if (found == SimpleServiceResult.Failure)
                {
                    await ReplyErrorLocalized("command_failed").ConfigureAwait(false);
                }
                else
                {
                    await ReplyConfirmLocalized("name_filters_all_removed").ConfigureAwait(false);
                }
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageGuild)]
            public async Task ListNameFilters()
            {
                List<string> result = await _service.ListNameFilters(Context.Guild.Id);
                if (result == null)
                {
                    await ReplyErrorLocalized("command_failed").ConfigureAwait(false);
                }
                else
                {
                    await ReplyConfirmLocalized("name_filters_list", string.Join(", ", result)).ConfigureAwait(false);
                }
            }
        }
    }
}
