using System.Collections.Generic;

namespace SwiftLeap.RulesConnector
{
    public class QueryResult
    {
        public IDictionary<string, string> Select { get; set; } = new Dictionary<string, string>(0);

        public string RuleCode { get; set; } = string.Empty;

        public string RuleId { get; set; } = string.Empty;

        public int Version { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public int Severity { get; set; }

        public string MappedCode { get; set; } = string.Empty;
    }
}