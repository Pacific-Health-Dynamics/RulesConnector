using System;

namespace SwiftLeap.RulesConnector
{
    public class QueryException : Exception, IError
    {
        public QueryException()
        {
        }

        public QueryException(IError error) : base(error.Message)
        {
            Code = error.Code;
            Reference = error.Reference;
        }

        public QueryException(int code, string message) : base(message)
        {
            Code = code;
            Reference = "";
        }


        public QueryException(string message) : base(message)
        {
            Code = 500;
            Reference = "";
        }

        public QueryException(string message, Exception cause) : base(message, cause)
        {
            Code = 500;
            Reference = "";
        }

        public int Code { get; set; }

        public string Reference { get; set; }
    }
}