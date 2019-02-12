using System.Collections.Generic;

namespace SwiftLeap.RulesConnector
{
    public class QueryResult
    {
        public IDictionary<string, string> Select { get; set; }

        public string RuleCode { get; set; }

        public string RuleId { get; set; }

        public string Name { get; set; }

        public string Message { get; set; }

        public int Severity { get; set; }

        public string MappedCode { get; set; }
    }
}