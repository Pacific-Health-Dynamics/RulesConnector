using System;

namespace SwiftLeap.RulesConnector
{
    public interface FieldFormatter
    {
        string Format(object value);
    }

    public class DefaultFieldFormatter : FieldFormatter
    {
        public string Format(object value)
        {
            switch (value)
            {
                case null:
                    return null;
                case DateTime date:
                    return date.ToString("dd/MM/yyyy");
                default:
                    return value.ToString();
            }
        }
    }
}