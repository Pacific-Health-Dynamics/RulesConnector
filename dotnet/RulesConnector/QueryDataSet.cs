using System.Collections.Generic;

namespace SwiftLeap.RulesConnector
{
    public class QueryDataSet
    {
        public string Name { get; set; }

        public IEnumerable<QueryRow> Rows { get; set; } = new List<QueryRow>();
    }
}