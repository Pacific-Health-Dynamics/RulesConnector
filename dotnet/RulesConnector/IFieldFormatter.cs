using System;
using System.Globalization;

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
                case decimal num:
                    return num.ToString(CultureInfo.InvariantCulture);
                case float num:
                    return num.ToString(CultureInfo.InvariantCulture);
                case double num:
                    return num.ToString(CultureInfo.InvariantCulture);
                case DateTime date:
                    return date.ToString("dd/MM/yyyy");
                default:
                    return value.ToString().Trim();
            }
        }
    }
}