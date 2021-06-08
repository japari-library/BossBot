using NadekoBot.Core.Services.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace NadekoBot.Core.Services.Database.Repositories.Impl
{
    public class AutoRefusedGuildUsernameTogglesRepository : Repository<AutoRefusedGuildUsernameToggle>, IAutoRefusedGuildUsernameTogglesRepository
    {
        public AutoRefusedGuildUsernameTogglesRepository(DbContext context) : base(context)
        {
        }

        private AutoRefusedGuildUsernameToggle CreateInitialToggle(ulong guildId)
        {
            AutoRefusedGuildUsernameToggle newToggle = new AutoRefusedGuildUsernameToggle
            {
                GuildId = guildId,
                AutoRefuse = false
            };
            _set.Add(newToggle);

            return newToggle;
        }

        public bool GetToggleStatus(ulong guildId)
        {
            AutoRefusedGuildUsernameToggle currentToggle = _set.FirstOrDefault(x => x.GuildId == guildId);
            if (currentToggle == null)
            {
                currentToggle = CreateInitialToggle(guildId);
            }

            return currentToggle.AutoRefuse;
        }

        public bool ToggleAutoRefuse(ulong guildId)
        {
            AutoRefusedGuildUsernameToggle currentToggle = _set.FirstOrDefault(x => x.GuildId == guildId);
            if (currentToggle == null)
            {
                currentToggle = CreateInitialToggle(guildId);
            }

            currentToggle.AutoRefuse = !currentToggle.AutoRefuse;
            return currentToggle.AutoRefuse;
        }
    }
}
