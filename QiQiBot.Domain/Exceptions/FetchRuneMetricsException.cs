namespace QiQiBot.Exceptions
{
    public class FetchRuneMetricsException : Exception
    {
        public FetchRuneMetricsException(string exception) : base($"Error fetching RuneMetrics profile: {exception}")
        {
        }
    }
}
