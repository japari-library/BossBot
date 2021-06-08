namespace NadekoBot.Core.Services.Database.Repositories
{
    public interface IAutoRefusedGuildUsernameTogglesRepository
    {
        bool GetToggleStatus(ulong guildId);
        bool ToggleAutoRefuse(ulong guildId);
    }
}
