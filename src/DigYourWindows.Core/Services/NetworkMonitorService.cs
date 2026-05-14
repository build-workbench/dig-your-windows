using System.Net.NetworkInformation;

namespace DigYourWindows.Core.Services;

/// <summary>
/// Provides real-time network traffic monitoring with rate calculation.
/// </summary>
public interface INetworkMonitorService
{
    /// <summary>
    /// Gets the current download rate in MB/s.
    /// </summary>
    double DownloadMBps { get; }

    /// <summary>
    /// Gets the current upload rate in MB/s.
    /// </summary>
    double UploadMBps { get; }

    /// <summary>
    /// Gets the network traffic history timestamps.
    /// </summary>
    IReadOnlyList<DateTime> HistoryTimes { get; }

    /// <summary>
    /// Gets the download rate history in MB/s.
    /// </summary>
    IReadOnlyList<double> HistoryDownload { get; }

    /// <summary>
    /// Gets the upload rate history in MB/s.
    /// </summary>
    IReadOnlyList<double> HistoryUpload { get; }

    /// <summary>
    /// Updates network traffic statistics.
    /// Should be called periodically (e.g., every second).
    /// </summary>
    void Update();

    /// <summary>
    /// Resets all network monitoring state.
    /// </summary>
    void Reset();
}

/// <summary>
/// Implementation of network monitor service that tracks traffic rates and history.
/// </summary>
public sealed class NetworkMonitorService : INetworkMonitorService, IDisposable
{
    private readonly ILogService _log;
    private readonly int _historyCapacity;

    private long? _lastBytesReceived;
    private long? _lastBytesSent;
    private DateTimeOffset? _lastSampleTime;

    private readonly Queue<DateTime> _historyTimes = new();
    private readonly Queue<double> _historyDownload = new();
    private readonly Queue<double> _historyUpload = new();

    private double _downloadMBps;
    private double _uploadMBps;

    private bool _disposed;

    public NetworkMonitorService(ILogService log, int historyCapacity = 60)
    {
        _log = log;
        _historyCapacity = historyCapacity;
    }

    public double DownloadMBps => _downloadMBps;
    public double UploadMBps => _uploadMBps;

    public IReadOnlyList<DateTime> HistoryTimes => _historyTimes.ToArray();
    public IReadOnlyList<double> HistoryDownload => _historyDownload.ToArray();
    public IReadOnlyList<double> HistoryUpload => _historyUpload.ToArray();

    public void Update()
    {
        try
        {
            var now = DateTimeOffset.Now;
            var (bytesReceived, bytesSent) = GetTotalBytes();

            if (TryInitializeSample(now, bytesReceived, bytesSent))
            {
                return;
            }

            if (!TryCalculateRates(now, bytesReceived, bytesSent, out var downloadMBps, out var uploadMBps))
            {
                return;
            }

            _downloadMBps = downloadMBps;
            _uploadMBps = uploadMBps;
            UpdateSampleState(now, bytesReceived, bytesSent);
            AppendHistory(now.LocalDateTime, downloadMBps, uploadMBps);
        }
        catch (Exception ex)
        {
            _log.Warn($"更新网络流量失败: {ex.Message}");
        }
    }

    public void Reset()
    {
        _lastBytesReceived = null;
        _lastBytesSent = null;
        _lastSampleTime = null;
        _downloadMBps = 0d;
        _uploadMBps = 0d;
        _historyTimes.Clear();
        _historyDownload.Clear();
        _historyUpload.Clear();
    }

    private bool TryInitializeSample(DateTimeOffset now, long bytesReceived, long bytesSent)
    {
        if (_lastSampleTime is not null && _lastBytesReceived is not null && _lastBytesSent is not null)
        {
            return false;
        }

        UpdateSampleState(now, bytesReceived, bytesSent);
        _downloadMBps = 0d;
        _uploadMBps = 0d;
        AppendHistory(now.LocalDateTime, 0d, 0d);
        return true;
    }

    private bool TryCalculateRates(
        DateTimeOffset now,
        long bytesReceived,
        long bytesSent,
        out double downloadMBps,
        out double uploadMBps)
    {
        downloadMBps = 0d;
        uploadMBps = 0d;

        if (_lastSampleTime is null || _lastBytesReceived is null || _lastBytesSent is null)
        {
            return false;
        }

        var dtSeconds = (now - _lastSampleTime.Value).TotalSeconds;
        if (dtSeconds <= 0)
        {
            return false;
        }

        var deltaReceived = Math.Max(0L, bytesReceived - _lastBytesReceived.Value);
        var deltaSent = Math.Max(0L, bytesSent - _lastBytesSent.Value);

        downloadMBps = deltaReceived / dtSeconds / 1024d / 1024d;
        uploadMBps = deltaSent / dtSeconds / 1024d / 1024d;
        return true;
    }

    private void UpdateSampleState(DateTimeOffset now, long bytesReceived, long bytesSent)
    {
        _lastSampleTime = now;
        _lastBytesReceived = bytesReceived;
        _lastBytesSent = bytesSent;
    }

    private void AppendHistory(DateTime time, double downloadMBps, double uploadMBps)
    {
        _historyTimes.Enqueue(time);
        _historyDownload.Enqueue(downloadMBps);
        _historyUpload.Enqueue(uploadMBps);

        while (_historyTimes.Count > _historyCapacity)
        {
            _historyTimes.Dequeue();
            _historyDownload.Dequeue();
            _historyUpload.Dequeue();
        }
    }

    private static (long BytesReceived, long BytesSent) GetTotalBytes()
    {
        long received = 0;
        long sent = 0;

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (nic.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                nic.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
            {
                continue;
            }

            try
            {
                var stats = nic.GetIPv4Statistics();
                received += stats.BytesReceived;
                sent += stats.BytesSent;
            }
            catch (Exception)
            {
                // Individual NIC failures are non-critical
            }
        }

        return (received, sent);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _historyTimes.Clear();
        _historyDownload.Clear();
        _historyUpload.Clear();
        GC.SuppressFinalize(this);
    }
}
