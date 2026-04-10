namespace QiQiBot.Exceptions
{
    public class GuildNotFoundException : Exception
    {
        public GuildNotFoundException(ulong guildId) : base($"No guild found with guildId {guildId}")
        {
        }
    }
}
