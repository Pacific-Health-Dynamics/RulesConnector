using System.Collections.Generic;
using System.Linq;

namespace SwiftLeap.RulesConnector
{
    internal class SchemaDataSet
    {
        public SchemaDataSet()
        {
        }

        public SchemaDataSet(string name, IEnumerable<SchemaColumnDef> columns)
        {
            Name = name;
            Columns = columns.ToList();
        }

        public string Name { get; set; }

        public IList<SchemaColumnDef> Columns { get; } = new List<SchemaColumnDef>(0);
    }
}