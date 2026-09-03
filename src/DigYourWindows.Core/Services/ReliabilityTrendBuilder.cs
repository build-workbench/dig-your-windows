using DigYourWindows.Core.Models;

namespace DigYourWindows.Core.Services;

/// <summary>
/// One day of reliability record counts, broken down by failure category.
/// </summary>
public sealed record ReliabilityTrendDayCounts(
    DateTime Day,
    int Total,
    int ApplicationFailures,
    int WindowsFailures,
    int OtherFailures,
    int Unknown);

/// <summary>
/// Pure computation that aggregates reliability records into per-day trend counts.
/// Mirrors the Windows Reliability Monitor categories:
/// 1 = 应用程序故障, 2 = Windows 故障, 3 = 其他故障; anything else (incl. null) counts as 未知.
/// </summary>
public static class ReliabilityTrendBuilder
{
    public static IReadOnlyList<ReliabilityTrendDayCounts> BuildDailyCounts(
        IEnumerable<ReliabilityRecordData> records,
        int daysBack)
    {
        var materialized = (records ?? throw new ArgumentNullException(nameof(records))).ToList();

        var endDate = DateTime.Today;
        DateTime startDate;

        if (daysBack <= 0)
        {
            // Without a window the trend spans from the earliest record date; with no
            // records at all the range collapses to today so callers still get one bucket.
            startDate = materialized.Count > 0
                ? materialized.Min(x => x.Timestamp.Date)
                : endDate;
        }
        else
        {
            startDate = endDate.AddDays(-(daysBack - 1));
        }

        var dayCount = (endDate - startDate).Days + 1;
        var result = new List<ReliabilityTrendDayCounts>(dayCount);

        for (var i = 0; i < dayCount; i++)
        {
            var day = startDate.AddDays(i);
            var dayRecords = materialized.Where(r => r.Timestamp.Date == day.Date).ToList();

            result.Add(new ReliabilityTrendDayCounts(
                Day: day,
                Total: dayRecords.Count,
                ApplicationFailures: CountCategory(dayRecords, expected: 1),
                WindowsFailures: CountCategory(dayRecords, expected: 2),
                OtherFailures: CountCategory(dayRecords, expected: 3),
                Unknown: dayRecords.Count(r => IsUnknownCategory(r.RecordType))));
        }

        return result;
    }

    private static int CountCategory(List<ReliabilityRecordData> dayRecords, int expected)
    {
        return dayRecords.Count(r => r.RecordType == expected);
    }

    private static bool IsUnknownCategory(int? recordType)
    {
        return !recordType.HasValue || (recordType is not 1 and not 2 and not 3);
    }
}
