namespace QiQiBot.Services
{
    public interface IClanEventService
    {
        public Task SendPlayerJoinEvent(ulong guildId, List<string> playerNames);
        public Task SendPlayerLeftEvent(ulong guildId, List<string> playerNames);
        public Task SendPlayerRenameEvent(ulong guildId, List<(string OldName, string NewName)> renames);
    }
}
