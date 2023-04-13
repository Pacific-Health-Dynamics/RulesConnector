using System;
using System.Linq;
using System.Reflection;

namespace SwiftLeap.RulesConnector
{
    internal class SchemaColumnDef : IFieldFormatter
    {
        public static readonly IFieldFormatter DefaultFieldFormatter = new DefaultFieldFormatter();

        public SchemaColumnDef(PropertyInfo prop)
        {
            Method = prop;

            var field = prop.GetCustomAttribute<QueryFieldAttribute>();


            if (field != null)
            {
                if (field.Formatter == typeof(DefaultFieldFormatter))
                    Formatter = DefaultFieldFormatter;
                else
                    Formatter = (IFieldFormatter) Activator.CreateInstance(field.Formatter);
                Name = field.Alias;
                Type = field.FieldType;
                Description = field.Description;
            }
            else
            {
                Formatter = DefaultFieldFormatter;
            }

            if (Type == FieldType.NULL)
                Type = TypeFromType(prop.PropertyType);

            if (string.IsNullOrWhiteSpace(Name))
                Name = ToPropName(prop.Name);
        }

        public string Name { get; set; } = string.Empty;
        public string Description { get; } = string.Empty;
        public FieldType Type { get; } = FieldType.NULL;
        private IFieldFormatter Formatter { get; }
        private PropertyInfo Method { get; }

        public string Format(object value)
        {
            return Formatter.Format(value);
        }

        private static bool IsNumericType(Type type)
        {
            switch (System.Type.GetTypeCode(type))
            {
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        private static FieldType TypeFromType(Type t)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                t = t.GenericTypeArguments.First();
            
            if (IsNumericType(t))
                return FieldType.NUMBER;
            if (typeof(DateTime).IsAssignableFrom(t))
                return FieldType.DATE;
            if (typeof(bool).IsAssignableFrom(t))
                return FieldType.BOOLEAN;
            return FieldType.WORD;
        }

        private static string ToPropName(string name)
        {
            return char.ToLower(name[0]) + name.Substring(1);
        }

        public object? Invoke(object? o)
        {
            return Method.GetMethod.Invoke(o, null);
        }
    }
}