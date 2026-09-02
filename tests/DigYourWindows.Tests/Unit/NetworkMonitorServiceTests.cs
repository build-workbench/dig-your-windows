using DigYourWindows.Core.Services;

namespace DigYourWindows.Tests.Unit;

/// <summary>
/// Unit tests for NetworkMonitorService.
/// Verifies rate calculation and history behavior through the public Update() API.
/// </summary>
public class NetworkMonitorServiceTests
{
    private sealed class StubLogService : ILogService
    {
        public List<string> WarnMessages { get; } = [];
        public void Info(string message) { }
        public void Warn(string message) => WarnMessages.Add(message);
        public void LogError(string message, Exception? exception = null) { }
    }

    private readonly StubLogService _log = new();

    [Fact]
    public void UpdateFirstCallShouldRecordZeroRateSample()
    {
        // Arrange
        using var service = new NetworkMonitorService(_log);

        // Act
        service.Update();

        // Assert - first sample initializes state, rate is zero, history gets one entry
        Assert.Equal(0d, service.DownloadMBps);
        Assert.Equal(0d, service.UploadMBps);
        Assert.Single(service.HistoryTimes);
        Assert.Single(service.HistoryDownload);
        Assert.Single(service.HistoryUpload);
        Assert.All(service.HistoryDownload, v => Assert.Equal(0d, v));
        Assert.All(service.HistoryUpload, v => Assert.Equal(0d, v));
    }

    [Fact]
    public void UpdateRepeatedCallsShouldNotWarnUnderNormalConditions()
    {
        // Arrange
        using var service = new NetworkMonitorService(_log);

        // Act
        for (var i = 0; i < 5; i++)
        {
            service.Update();
            Thread.Sleep(10);
        }

        // Assert - under normal conditions, no warnings should be logged
        Assert.Empty(_log.WarnMessages);
    }

    [Fact]
    public void ResetShouldClearRatesAndHistory()
    {
        // Arrange
        using var service = new NetworkMonitorService(_log);
        service.Update();
        Thread.Sleep(10);
        service.Update();

        // Act
        service.Reset();

        // Assert
        Assert.Equal(0d, service.DownloadMBps);
        Assert.Equal(0d, service.UploadMBps);
        Assert.Empty(service.HistoryTimes);
        Assert.Empty(service.HistoryDownload);
        Assert.Empty(service.HistoryUpload);

        // Next Update starts a fresh sample (zero-rate initialization)
        service.Update();
        Assert.Equal(0d, service.DownloadMBps);
        Assert.Single(service.HistoryTimes);
    }

    [Fact]
    public void HistoryCapacityShouldBeBounded()
    {
        // Arrange
        using var service = new NetworkMonitorService(_log, historyCapacity: 5);

        // Act
        for (var i = 0; i < 20; i++)
        {
            service.Update();
            Thread.Sleep(5);
        }

        // Assert
        Assert.True(service.HistoryTimes.Count <= 5);
        Assert.True(service.HistoryDownload.Count <= 5);
        Assert.True(service.HistoryUpload.Count <= 5);
        Assert.Equal(service.HistoryTimes.Count, service.HistoryDownload.Count);
        Assert.Equal(service.HistoryDownload.Count, service.HistoryUpload.Count);
    }

    [Fact]
    public void DisposeShouldBeIdempotent()
    {
        // Arrange
        var service = new NetworkMonitorService(_log);

        // Act & Assert
        service.Dispose();
        Record.Exception(() => service.Dispose());
    }
}
