namespace NadekoBot.Core.Services.Database.Models
{
    public class AutoRefusedGuildUsername : DbEntity
    {
        public ulong GuildId { get; set; }

        public string BannedText { get; set; }
    }
}
