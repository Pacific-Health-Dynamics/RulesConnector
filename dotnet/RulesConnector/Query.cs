using System.Collections.Generic;

namespace SwiftLeap.RulesConnector
{
    internal class Query
    {
        public IList<QuerySelect> Select { get; set; } = new List<QuerySelect>();

        public IList<QueryDataSet> DataSets { get; set; } = new List<QueryDataSet>();
    }
}