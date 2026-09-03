using DigYourWindows.Core.Models;
using DigYourWindows.Core.Services;

namespace DigYourWindows.Tests.Unit;

/// <summary>
/// Unit tests for ReliabilityTrendBuilder — pure aggregation of reliability
/// records into per-day trend counts.
/// </summary>
public class ReliabilityTrendBuilderTests
{
    private static ReliabilityRecordData Record(DateTime timestamp, int? recordType)
    {
        return new ReliabilityRecordData
        {
            Timestamp = timestamp,
            SourceName = "TestSource",
            Message = "Test reliability record",
            RecordType = recordType
        };
    }

    [Fact]
    public void BuildDailyCounts_WithFixedWindow_IncludesTodayAndExcludesOlderRecords()
    {
        var today = DateTime.Today;
        var records = new List<ReliabilityRecordData>
        {
            Record(today, 1),
            Record(today.AddDays(-1), 2),
            Record(today.AddDays(-2), 3),
            Record(today.AddDays(-30), 1),
        };

        var trend = ReliabilityTrendBuilder.BuildDailyCounts(records, daysBack: 3);

        Assert.Equal(3, trend.Count);
        Assert.Equal(today.AddDays(-2).Date, trend[0].Day);
        Assert.Equal(today.Date, trend[2].Day);
        Assert.Equal(1, trend[0].Total);
        Assert.Equal(1, trend[1].Total);
        Assert.Equal(1, trend[2].Total);
    }

    [Fact]
    public void BuildDailyCounts_WithNonPositiveDaysBack_SpansFromEarliestRecord()
    {
        var today = DateTime.Today;
        var earliest = today.AddDays(-5);
        var records = new List<ReliabilityRecordData>
        {
            Record(today, 1),
            Record(earliest, 2),
        };

        var trend = ReliabilityTrendBuilder.BuildDailyCounts(records, daysBack: 0);

        Assert.Equal(6, trend.Count);
        Assert.Equal(earliest.Date, trend[0].Day);
        Assert.Equal(today.Date, trend[^1].Day);
    }

    [Fact]
    public void BuildDailyCounts_WithNoRecordsAndNoWindow_ReturnsSingleTodayBucket()
    {
        var trend = ReliabilityTrendBuilder.BuildDailyCounts(new List<ReliabilityRecordData>(), daysBack: 0);

        var point = Assert.Single(trend);
        Assert.Equal(DateTime.Today, point.Day.Date);
        Assert.Equal(0, point.Total);
    }

    [Fact]
    public void BuildDailyCounts_MapsRecordTypesToCategories()
    {
        var today = DateTime.Today;
        var records = new List<ReliabilityRecordData>
        {
            Record(today, 1),                       // 应用程序故障
            Record(today, 2),                       // Windows 故障
            Record(today, 3),                       // 其他故障
            Record(today, null),                    // 未知
            Record(today, 99),                      // 未知（超出已知类别）
        };

        var point = Assert.Single(ReliabilityTrendBuilder.BuildDailyCounts(records, daysBack: 1));

        Assert.Equal(5, point.Total);
        Assert.Equal(1, point.ApplicationFailures);
        Assert.Equal(1, point.WindowsFailures);
        Assert.Equal(1, point.OtherFailures);
        Assert.Equal(2, point.Unknown);
    }

    [Fact]
    public void BuildDailyCounts_AggregatesMultipleRecordsPerDay()
    {
        var today = DateTime.Today;
        var records = new List<ReliabilityRecordData>
        {
            Record(today.AddHours(1), 1),
            Record(today.AddHours(5), 1),
            Record(today.AddHours(9), 2),
        };

        var point = Assert.Single(ReliabilityTrendBuilder.BuildDailyCounts(records, daysBack: 1));

        Assert.Equal(3, point.Total);
        Assert.Equal(2, point.ApplicationFailures);
        Assert.Equal(1, point.WindowsFailures);
    }

    [Fact]
    public void BuildDailyCounts_DaysWithinWindowWithoutRecords_AreZeroBuckets()
    {
        var today = DateTime.Today;
        var records = new List<ReliabilityRecordData> { Record(today, 1) };

        var trend = ReliabilityTrendBuilder.BuildDailyCounts(records, daysBack: 7);

        Assert.Equal(7, trend.Count);
        Assert.Equal(1, trend[^1].Total);
        Assert.All(trend.Take(6), p => Assert.Equal(0, p.Total));
    }
}
