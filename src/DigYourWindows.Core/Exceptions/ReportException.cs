namespace DigYourWindows.Core.Exceptions;

/// <summary>
/// Exception thrown when report generation fails.
/// </summary>
public class ReportException : Exception
{
    /// <summary>
    /// The type of report error that occurred.
    /// </summary>
    public ReportErrorType ErrorType { get; }

    public ReportException(string message) : base(message)
    {
        ErrorType = ReportErrorType.Unknown;
    }

    public ReportException(string message, Exception innerException)
        : base(message, innerException)
    {
        ErrorType = ReportErrorType.Unknown;
    }

    public ReportException(ReportErrorType errorType, string message)
        : base(message)
    {
        ErrorType = errorType;
    }

    public ReportException(ReportErrorType errorType, string message, Exception inner)
        : base(message, inner)
    {
        ErrorType = errorType;
    }

    /// <summary>
    /// Creates a Serialization exception.
    /// </summary>
    public static ReportException Serialization(string message) =>
        new(ReportErrorType.Serialization, $"Serialization error: {message}");

    /// <summary>
    /// Creates an InvalidData exception.
    /// </summary>
    public static ReportException InvalidData(string message) =>
        new(ReportErrorType.InvalidData, $"Invalid report data: {message}");
}

/// <summary>
/// Types of report errors that can occur.
/// </summary>
public enum ReportErrorType
{
    Unknown,
    Serialization,
    InvalidData
}
