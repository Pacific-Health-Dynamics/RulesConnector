using System;

namespace SwiftLeap.RulesConnector
{
    public sealed class QueryFieldAttribute : Attribute
    {
        public string Alias { get; set; }

        public bool Ignore { get; set; }

        public Type Formatter { get; set; }
    }
}