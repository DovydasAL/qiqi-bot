using System.Net;

namespace QiQiBot.Exceptions
{
    public class FetchMembersException : Exception
    {
        public FetchMembersException(HttpStatusCode statusCode) : base($"Status code: {statusCode}")
        {
        }
    }
}
