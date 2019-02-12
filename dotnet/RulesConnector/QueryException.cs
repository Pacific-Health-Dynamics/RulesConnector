using System;

namespace SwiftLeap.RulesConnector
{
    public class QueryException : Exception
    {
        public QueryException()
        {
        }

        public QueryException(string message) : base(message)
        {
        }

        public QueryException(string message, Exception cause) : base(message, cause)
        {
        }
    }
}