using System.Collections.Generic;

namespace SwiftLeap.RulesConnector
{
    internal class QueryRow
    {
        public IDictionary<string, string> Values { get; set; } = new Dictionary<string, string>(0);
    }
}