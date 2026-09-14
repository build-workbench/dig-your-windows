using System.Diagnostics;

namespace DigYourWindows.Core.Services;

/// <summary>
/// A single user-space process with its sampled resource usage.
/// </summary>
public sealed record ProcessInfoData
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;

    /// <summary>Percent of total CPU capacity (100 = all cores busy), 0 on the first sample.</summary>
    public double CpuPercent { get; init; }

    /// <summary>Physical working set in megabytes.</summary>
    public double MemoryMB { get; init; }
}

/// <summary>
/// Samples running processes and reports the top consumers by CPU (fallback: memory
/// before the second sample exists). CPU percent is derived from TotalProcessorTime
/// deltas between consecutive samples, normalized by logical core count.
/// </summary>
public interface IProcessMonitorService
{
    IReadOnlyList<ProcessInfoData> GetTopProcesses(int topCount = 10);
}

public sealed class ProcessMonitorService : IProcessMonitorService
{
    private readonly ILogService _log;
    private readonly Func<IReadOnlyList<ProcessSample>> _snapshotSource;
    private readonly int _coreCount;
    private Dictionary<int, TimeSpan> _lastCpuTimes = new();
    private DateTime? _lastSampleTime;

    public ProcessMonitorService(ILogService log)
        : this(log, DefaultSnapshotSource, Environment.ProcessorCount)
    {
    }

    /// <summary>Test/diagnostic constructor with an injectable snapshot source and core count.</summary>
    public ProcessMonitorService(
        ILogService log,
        Func<IReadOnlyList<ProcessSample>> snapshotSource,
        int coreCount)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _snapshotSource = snapshotSource ?? throw new ArgumentNullException(nameof(snapshotSource));
        _coreCount = Math.Max(1, coreCount);
    }

    public IReadOnlyList<ProcessInfoData> GetTopProcesses(int topCount = 10)
    {
        return GetTopProcesses(topCount, DateTime.UtcNow);
    }

    public IReadOnlyList<ProcessInfoData> GetTopProcesses(int topCount, DateTime sampleTime)
    {
        List<ProcessSample> snapshot;
        try
        {
            snapshot = _snapshotSource().ToList();
        }
        catch (Exception ex)
        {
            _log.Warn($"Enumerating processes failed: {ex.Message}");
            return Array.Empty<ProcessInfoData>();
        }

        var wallSeconds = _lastSampleTime is { } prevTime
            ? (sampleTime - prevTime).TotalSeconds
            : 0d;

        var results = new List<ProcessInfoData>(snapshot.Count);
        var nextSamples = new Dictionary<int, TimeSpan>(snapshot.Count);

        foreach (var s in snapshot)
        {
            nextSamples[s.Id] = s.CpuTime;

            double cpuPercent = 0d;
            if (wallSeconds > 0 && _lastCpuTimes.TryGetValue(s.Id, out var prevCpu))
            {
                var cpuSeconds = (s.CpuTime - prevCpu).TotalSeconds;
                if (cpuSeconds > 0)
                {
                    cpuPercent = Math.Min(100d, cpuSeconds / (wallSeconds * _coreCount) * 100d);
                }
            }

            results.Add(new ProcessInfoData
            {
                Id = s.Id,
                Name = s.Name,
                CpuPercent = cpuPercent,
                MemoryMB = s.WorkingSetBytes / 1024d / 1024d
            });
        }

        _lastCpuTimes = nextSamples;
        _lastSampleTime = sampleTime;

        return results
            .OrderByDescending(p => p.CpuPercent)
            .ThenByDescending(p => p.MemoryMB)
            .ThenBy(p => p.Name)
            .Take(Math.Max(0, topCount))
            .ToList();
    }

    private static IReadOnlyList<ProcessSample> DefaultSnapshotSource()
    {
        var list = new List<ProcessSample>();
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                try
                {
                    list.Add(new ProcessSample(
                        process.Id,
                        process.ProcessName,
                        SafeCpuTime(process),
                        SafeWorkingSet(process)));
                }
                catch (Exception)
                {
                    // System processes may vanish or deny access mid-enumeration; skip them.
                }
            }
        }

        return list;
    }

    private static TimeSpan SafeCpuTime(Process process)
    {
        try
        {
            return process.TotalProcessorTime;
        }
        catch (Exception)
        {
            return TimeSpan.Zero;
        }
    }

    private static long SafeWorkingSet(Process process)
    {
        try
        {
            return process.WorkingSet64;
        }
        catch (Exception)
        {
            return 0;
        }
    }
}

/// <summary>One point-in-time process sample, decoupled from System.Diagnostics.Process for testability.</summary>
public sealed record ProcessSample(int Id, string Name, TimeSpan CpuTime, long WorkingSetBytes);
