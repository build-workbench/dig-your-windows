using System.Globalization;
using DigYourWindows.Core.Models;
using DigYourWindows.Core.Services;

namespace DigYourWindows.Tests.Unit;

/// <summary>
/// Unit tests for the CSV report export (structure, escaping, invariant numbers).
/// </summary>
public class ReportServiceCsvTests
{
    private static DiagnosticData CreateData(string computerName = "TEST-PC")
    {
        return new DiagnosticData
        {
            Hardware = new HardwareData
            {
                ComputerName = computerName,
                OsVersion = "Windows 11 Pro",
                CpuBrand = "AMD Ryzen 7",
                CpuCores = 8,
                TotalMemory = 16UL * 1024UL * 1024UL * 1024UL,
                Disks = new List<DiskInfoData>
                {
                    new() { Name = "C:", TotalSpace = 100, AvailableSpace = 50 }
                }
            },
            Events = new List<LogEventData> { new() { Message = "e" } },
            Reliability = new List<ReliabilityRecordData> { new() },
            Performance = new PerformanceAnalysisData
            {
                SystemHealthScore = 87.5,
                StabilityScore = 30,
                PerformanceScore = 95,
                MemoryUsageScore = 90,
                DiskHealthScore = 82,
                CriticalIssuesCount = 1,
                WarningsCount = 21
            },
            CollectedAt = new DateTime(2026, 9, 13, 8, 30, 0, DateTimeKind.Utc)
        };
    }

    [Fact]
    public void GenerateCsvReport_ContainsHeaderAndKeyValues()
    {
        var service = new ReportService();
        var csv = service.GenerateCsvReport(CreateData());
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        Assert.StartsWith("项目,数值", lines[0], StringComparison.Ordinal);
        Assert.Contains(lines, l => l.StartsWith("计算机名,", StringComparison.Ordinal));
        Assert.Contains(lines, l => l.Contains("TEST-PC", StringComparison.Ordinal));
        Assert.Contains(lines, l => l.StartsWith("系统健康评分,", StringComparison.Ordinal));
        Assert.Contains(lines, l => l.StartsWith("警告数量,", StringComparison.Ordinal));
    }

    [Fact]
    public void GenerateCsvReport_UsesInvariantDecimalSeparator()
    {
        // 87.5 must serialize as "87.5" regardless of the machine culture
        var service = new ReportService();
        var csv = service.GenerateCsvReport(CreateData());

        Assert.Contains("系统健康评分,87.5", csv);
        Assert.DoesNotContain("87,5", csv);
    }

    [Fact]
    public void GenerateCsvReport_QuotesValuesContainingCommas()
    {
        var service = new ReportService();
        var data = CreateData(computerName: "PC,LAB-1");

        var csv = service.GenerateCsvReport(data);

        Assert.Contains("\"PC,LAB-1\"", csv);
    }

    [Fact]
    public void GenerateCsvReport_IncludesUtcCollectTime()
    {
        var service = new ReportService();
        var csv = service.GenerateCsvReport(CreateData());

        Assert.Contains("采集时间 (UTC),2026-09-13 08:30:00", csv);
    }
}
