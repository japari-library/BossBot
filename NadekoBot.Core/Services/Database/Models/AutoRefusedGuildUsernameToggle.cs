namespace NadekoBot.Core.Services.Database.Models
{
    public class AutoRefusedGuildUsernameToggle : DbEntity
    {
        public ulong GuildId { get; set; }

        public bool AutoRefuse { get; set; }
    }
}
