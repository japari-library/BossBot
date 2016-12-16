﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using NadekoBot.Attributes;
using NadekoBot.Extensions;
using NadekoBot.Services;
using NadekoBot.Services.Database.Models;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NadekoBot.Modules.Administration
{
    public partial class Administration
    {
        [Group]
        public class RepeatCommands
        {
            public static ConcurrentDictionary<ulong, RepeatRunner> repeaters { get; }

            public class RepeatRunner
            {
                private Logger _log { get; }

                private CancellationTokenSource source { get; set; }
                private CancellationToken token { get; set; }
                public Repeater Repeater { get; }
                public ITextChannel Channel { get; }

                public RepeatRunner(Repeater repeater, ITextChannel channel = null)
                {
                    _log = LogManager.GetCurrentClassLogger();
                    this.Repeater = repeater;
                    this.Channel = channel ?? NadekoBot.Client.GetGuild(repeater.GuildId)?.GetTextChannel(repeater.ChannelId);
                    if (Channel == null)
                        return;
                    Task.Run(Run);
                }


                private async Task Run()
                {
                    source = new CancellationTokenSource();
                    token = source.Token;
                    IUserMessage oldMsg = null;
                    try
                    {
                        while (!token.IsCancellationRequested)
                        {
                            await Task.Delay(Repeater.Interval, token).ConfigureAwait(false);
                            if (oldMsg != null)
                                try { await oldMsg.DeleteAsync(); } catch { }
                            try { oldMsg = await Context.Channel.SendMessageAsync("🔄 " + Repeater.Message).ConfigureAwait(false); } catch (Exception ex) { _log.Warn(ex); try { source.Cancel(); } catch { } }
                        }
                    }
                    catch (OperationCanceledException) { }
                }

                public void Reset()
                {
                    source.Cancel();
                    var t = Task.Run(Run);
                }

                public void Stop()
                {
                    source.Cancel();
                }
            }

            static RepeatCommands()
            {
                using (var uow = DbHandler.UnitOfWork())
                {
                    repeaters = new ConcurrentDictionary<ulong, RepeatRunner>(uow.Repeaters.GetAll().Select(r => new RepeatRunner(r)).Where(r => r != null).ToDictionary(r => r.Repeater.ChannelId));
                }
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            public async Task RepeatInvoke()
            {
                //var channel = (ITextChannel)Context.Channel;

                RepeatRunner rep;
                if (!repeaters.TryGetValue(channel.Id, out rep))
                {
                    await Context.Channel.SendErrorAsync("ℹ️ **No repeating message found on this server.**").ConfigureAwait(false);
                    return;
                }
                rep.Reset();
                await Context.Channel.SendMessageAsync("🔄 " + rep.Repeater.Message).ConfigureAwait(false);
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            public async Task Repeat()
            {
                //var channel = (ITextChannel)Context.Channel;
                RepeatRunner rep;
                if (repeaters.TryRemove(channel.Id, out rep))
                {
                    using (var uow = DbHandler.UnitOfWork())
                    {
                        uow.Repeaters.Remove(rep.Repeater);
                        await uow.CompleteAsync();
                    }
                    rep.Stop();
                    await Context.Channel.SendConfirmAsync("✅ **Stopped repeating a message.**").ConfigureAwait(false);
                }
                else
                    await Context.Channel.SendConfirmAsync("ℹ️ **No message is repeating.**").ConfigureAwait(false);
            }

            [NadekoCommand, Usage, Description, Aliases]
            [RequireContext(ContextType.Guild)]
            [RequireUserPermission(GuildPermission.ManageMessages)]
            public async Task Repeat(IUserMessage imsg, int minutes, [Remainder] string message)
            {
                //var channel = (ITextChannel)Context.Channel;

                if (minutes < 1 || minutes > 10080)
                    return;

                if (string.IsNullOrWhiteSpace(message))
                    return;

                RepeatRunner rep;

                rep = repeaters.AddOrUpdate(channel.Id, (cid) =>
                {
                    using (var uow = DbHandler.UnitOfWork())
                    {
                        var localRep = new Repeater
                        {
                            ChannelId = channel.Id,
                            GuildId = Context.Guild.Id,
                            Interval = TimeSpan.FromMinutes(minutes),
                            Message = message,
                        };
                        uow.Repeaters.Add(localRep);
                        uow.Complete();
                        return new RepeatRunner(localRep, channel);
                    }
                }, (cid, old) =>
                {
                    using (var uow = DbHandler.UnitOfWork())
                    {
                        old.Repeater.Message = message;
                        old.Repeater.Interval = TimeSpan.FromMinutes(minutes);
                        uow.Repeaters.Update(old.Repeater);
                        uow.Complete();
                    }
                    old.Reset();
                    return old;
                });

                await Context.Channel.SendConfirmAsync($"🔁 Repeating **\"{rep.Repeater.Message}\"** every `{rep.Repeater.Interval.Days} day(s), {rep.Repeater.Interval.Hours} hour(s) and {rep.Repeater.Interval.Minutes} minute(s)`.").ConfigureAwait(false);
            }
        }
    }
}
