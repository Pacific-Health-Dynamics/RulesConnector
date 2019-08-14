using System.Collections.Generic;

namespace SwiftLeap.RulesConnector
{
    internal class Schema
    {
        public IList<SchemaDataSet> DataSets = new List<SchemaDataSet>(0);

        public string Name { get; set; }
    }
}