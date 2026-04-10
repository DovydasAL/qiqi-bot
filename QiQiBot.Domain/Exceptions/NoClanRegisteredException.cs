namespace QiQiBot.Exceptions
{
    public class NoClanRegisteredException : Exception
    {
        public NoClanRegisteredException(ulong guildId) : base($"No clan registered for guild {guildId}")
        {
        }
    }
}
