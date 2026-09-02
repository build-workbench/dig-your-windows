namespace DigYourWindows.Core.Services;

/// <summary>
/// Centralizes application-wide configuration constants.
/// Single place to adjust logging, monitoring and formatting limits.
/// </summary>
public sealed class ConfigurationService
{
    public int MaxLogFiles { get; init; } = 7;

    public long MaxLogFileSizeBytes { get; init; } = 10 * 1024 * 1024;

    public int NetworkHistoryCapacity { get; init; } = 60;

    public int TimerIntervalSeconds { get; init; } = 1;

    public int EventMessageMaxLength { get; init; } = 100;
}
