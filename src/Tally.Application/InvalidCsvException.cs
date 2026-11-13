namespace Tally.Application;

public sealed class InvalidCsvException : Exception
{
    public InvalidCsvException(string message)
        : base(message)
    {
    }

    public InvalidCsvException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
