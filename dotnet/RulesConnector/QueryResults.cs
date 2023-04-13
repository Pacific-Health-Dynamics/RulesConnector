using System;
using System.Collections.Generic;

namespace SwiftLeap.RulesConnector
{
    public class QueryResults
    {
        public IEnumerable<QueryResult> Results { get; set; } = Array.Empty<QueryResult>();
    }
}