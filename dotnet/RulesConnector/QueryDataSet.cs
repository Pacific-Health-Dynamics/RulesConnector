using System.Collections.Generic;

namespace SwiftLeap.RulesConnector
{
    internal class QueryDataSet
    {
        public string Name { get; set; }

        public IEnumerable<QueryRow> Rows { get; set; } = new List<QueryRow>();
    }
}