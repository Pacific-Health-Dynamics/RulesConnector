using System;

namespace SwiftLeap.RulesConnector
{
    public sealed class QueryFieldAttribute : Attribute
    {
        public string Alias { get; set; } = "";

        public bool Ignore { get; set; }

        public FieldType FieldType { get; set; } = FieldType.NULL;

        public Type Formatter { get; set; } = typeof(DefaultFieldFormatter);

        public string Description { get; set; } = "";
    }
}