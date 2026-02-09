using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QiQiBot.Exceptions
{
    public class FetchMembersException : Exception
    {
        public FetchMembersException(HttpStatusCode statusCode) : base($"Status code: {statusCode}")
        {
        }
    }
}
