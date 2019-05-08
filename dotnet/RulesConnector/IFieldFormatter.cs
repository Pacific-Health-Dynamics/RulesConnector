using System;

namespace SwiftLeap.RulesConnector
{
    public interface IFieldFormatter
    {
        string Format(object value);
    }

    public class DefaultFieldFormatter : IFieldFormatter
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