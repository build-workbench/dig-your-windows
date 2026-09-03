using LibreHardwareMonitor.Hardware;

namespace DigYourWindows.Core.Services;

/// <summary>
/// Shared LibreHardwareMonitor Computer instance.
/// Avoids creating multiple heavyweight Computer objects for CPU/GPU monitoring.
/// </summary>
public interface IHardwareMonitorProvider : IDisposable
{
    Computer Computer { get; }

    /// <summary>
    /// Lock that must be held while updating/reading hardware sensors:
    /// LibreHardwareMonitor does not guarantee thread-safe concurrent updates
    /// (ring0 MSR access, driver handles) across hardware instances.
    /// </summary>
    object SyncRoot { get; }
}

public sealed class HardwareMonitorProvider : IHardwareMonitorProvider
{
    private readonly object _lock = new();
    private Computer? _computer;
    private bool _disposed;

    public Computer Computer
    {
        get
        {
            lock (_lock)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                return _computer ?? throw new ObjectDisposedException(nameof(HardwareMonitorProvider));
            }
        }
    }

    public object SyncRoot => _lock;

    public HardwareMonitorProvider()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true
        };
        try
        {
            _computer.Open();
        }
        catch
        {
            _computer.Close();
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _computer?.Close();
            _computer = null;
        }
    }
}
