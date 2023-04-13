using System.Collections.Generic;

namespace SwiftLeap.RulesConnector
{
    internal class QueryDataSet
    {
        public string Name { get; set; } = string.Empty;

        public IEnumerable<QueryRow> Rows { get; set; } = new List<QueryRow>();
    }
}