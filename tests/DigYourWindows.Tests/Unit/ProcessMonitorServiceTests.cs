using DigYourWindows.Core.Models;
using DigYourWindows.Core.Services;

namespace DigYourWindows.Tests.Unit;

/// <summary>
/// Unit tests for ProcessMonitorService CPU-percent math, top-N ordering and
/// dead-process pruning, using an injectable snapshot source.
/// </summary>
public class ProcessMonitorServiceTests
{
    private static readonly DateTime T0 = new(2026, 9, 13, 12, 0, 0, DateTimeKind.Utc);

    private static ProcessSample Sample(int id, string name, double cpuSeconds, long memoryMB) =>
        new(id, name, TimeSpan.FromSeconds(cpuSeconds), memoryMB * 1024L * 1024L);

    private sealed class LogSink : ILogService
    {
        public void Info(string message) { }
        public void Warn(string message) { }
        public void LogError(string message, Exception? exception = null) { }
    }

    [Fact]
    public void FirstSample_ReportsZeroCpuAndMemory()
    {
        var source = () => new List<ProcessSample>
        {
            Sample(1, "app", cpuSeconds: 5, memoryMB: 300)
        };
        var service = new ProcessMonitorService(new LogSink(), source, coreCount: 2);

        var top = service.GetTopProcesses(10, T0);

        var first = Assert.Single(top);
        Assert.Equal(0d, first.CpuPercent);
        Assert.Equal(300d, first.MemoryMB, precision: 0);
    }

    [Fact]
    public void SecondSample_ComputesCpuPercentNormalizedByCores()
    {
        // 0.5s of CPU across 1s wall on 2 cores = 25% of total capacity
        var samples = new List<ProcessSample>
        {
            Sample(1, "busy", cpuSeconds: 5.0, memoryMB: 300)
        };
        Func<IReadOnlyList<ProcessSample>> source = () => samples.ToList();
        var service = new ProcessMonitorService(new LogSink(), source, coreCount: 2);
        service.GetTopProcesses(10, T0);

        samples = [Sample(1, "busy", cpuSeconds: 5.5, memoryMB: 300)];
        var top = service.GetTopProcesses(10, T0.AddSeconds(1));

        Assert.Equal(25d, Assert.Single(top).CpuPercent, precision: 1);
    }

    [Fact]
    public void GetTopProcesses_OrdersByCpuThenMemory()
    {
        // high-cpu must appear in the first sample too, otherwise it has no
        // baseline and no CPU delta on the second sample.
        var first = new List<ProcessSample>
        {
            Sample(1, "low", 1, 100),
            Sample(2, "high-cpu", 1, 100)
        };
        Func<IReadOnlyList<ProcessSample>> source = () => first.ToList();
        // Dereference the variable inside the lambda so reassigning `source`
        // below actually swaps the next snapshot (see PrunesDeadProcesses).
        var service = new ProcessMonitorService(new LogSink(), () => source(), coreCount: 2);
        service.GetTopProcesses(10, T0);

        source = () => new List<ProcessSample>
        {
            Sample(2, "high-cpu", 4, 100),   // +3s cpu over 1s wall on 2 cores = 150% -> clamped 100
            Sample(3, "high-mem", 1, 2000),
            Sample(1, "low", 1, 100)
        };
        var top = service.GetTopProcesses(3, T0.AddSeconds(1));

        Assert.Equal(2, top[0].Id);       // clamped 100% cpu first
        Assert.Equal(3, top[1].Id);       // 0% cpu but 2 GB memory
        Assert.Equal(1, top[2].Id);
    }

    [Fact]
    public void GetTopProcesses_PrunesDeadProcesses()
    {
        var withDead = new List<ProcessSample>
        {
            Sample(1, "alive", 1, 100),
            Sample(2, "dead", 1, 100)
        };
        Func<IReadOnlyList<ProcessSample>> source = () => withDead;
        var service = new ProcessMonitorService(new LogSink(), () => source(), coreCount: 2);
        service.GetTopProcesses(10, T0);

        source = () => new List<ProcessSample> { Sample(1, "alive", 1, 100) };
        service.GetTopProcesses(10, T0.AddSeconds(1));

        // A new process reusing the dead PID must not inherit the stale CPU delta.
        source = () => new List<ProcessSample> { Sample(2, "reused-pid", 1, 100) };
        var top = service.GetTopProcesses(10, T0.AddSeconds(2));

        var reused = Assert.Single(top);
        Assert.Equal(0d, reused.CpuPercent);  // no stale delta applied
    }

    [Fact]
    public void GetTopProcesses_RespectsTopCount()
    {
        var source = () => Enumerable.Range(1, 25)
            .Select(i => Sample(i, $"p{i}", cpuSeconds: i, memoryMB: i * 10))
            .ToList();
        var service = new ProcessMonitorService(new LogSink(), source, coreCount: 2);
        service.GetTopProcesses(10, T0);

        var top = service.GetTopProcesses(10, T0.AddSeconds(1));

        Assert.Equal(10, top.Count);
        Assert.Equal(25, top[0].Id);   // largest cpu delta
        Assert.Equal(16, top[^1].Id);  // smallest delta within top 10
    }
}
