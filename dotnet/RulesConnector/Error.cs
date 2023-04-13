namespace SwiftLeap.RulesConnector
{
    public interface IError
    {
        string Message { get; }
        int Code { get; }
        string Reference { get; }
    }

    public class Error : IError
    {
        public string Message { get; set; } = string.Empty;

        public int Code { get; set; }

        public string Reference { get; set; } = string.Empty;
    }
}