namespace QiQiBot.Exceptions
{
    public class InvalidResetTimeException : Exception
    {
        public InvalidResetTimeException(string? time)
            : base($"Invalid cap reset time: {time}. Time must be in the format HH:mm and between 00:00 and 23:59.")
        {
        }
    }
}
