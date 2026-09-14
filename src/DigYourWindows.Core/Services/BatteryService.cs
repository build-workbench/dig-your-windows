using System.Management;

namespace DigYourWindows.Core.Services;

/// <summary>
/// Battery / power state snapshot for portable devices.
/// </summary>
public sealed record BatteryInfoData
{
    public bool HasBattery { get; init; }

    /// <summary>Remaining charge in percent (0-100), null when unknown.</summary>
    public int? ChargePercent { get; init; }

    /// <summary>Human-readable power source / charge state.</summary>
    public string PowerState { get; init; } = "未知";

    /// <summary>True when running on battery instead of AC power.</summary>
    public bool OnBattery { get; init; }

    /// <summary>Vendor-estimated remaining runtime in minutes; null when unknown.</summary>
    public int? EstimatedRunTimeMinutes { get; init; }
}

/// <summary>
/// Reads battery state from Win32_Battery (System.Management already referenced).
/// </summary>
public interface IBatteryService
{
    BatteryInfoData GetBatteryInfo();
}

public sealed class BatteryService : IBatteryService
{
    /// <summary>Win32_Battery sentinel value meaning "runtime unknown".</summary>
    private const int UnknownRunTime = 71582788;

    private readonly ILogService _log;
    private readonly Func<IReadOnlyList<(ushort? Status, int? ChargePercent, int? RunTimeMinutes)>> _source;

    public BatteryService(ILogService log)
        : this(log, ReadFromWmi)
    {
    }

    /// <summary>Test/diagnostic constructor with an injectable battery source.</summary>
    public BatteryService(
        ILogService log,
        Func<IReadOnlyList<(ushort? Status, int? ChargePercent, int? RunTimeMinutes)>> source)
    {
        _log = log ?? throw new ArgumentNullException(nameof(log));
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public BatteryInfoData GetBatteryInfo()
    {
        IReadOnlyList<(ushort? Status, int? ChargePercent, int? RunTimeMinutes)> batteries;
        try
        {
            batteries = _source();
        }
        catch (Exception ex)
        {
            _log.Warn($"获取电池信息失败: {ex.Message}");
            return new BatteryInfoData { HasBattery = false, PowerState = "未检测到电池" };
        }

        if (batteries.Count == 0)
        {
            return new BatteryInfoData { HasBattery = false, PowerState = "未检测到电池" };
        }

        var first = batteries[0];
        if (first.Status is null && first.ChargePercent is null)
        {
            return new BatteryInfoData { HasBattery = false, PowerState = "未检测到电池" };
        }

        var status = first.Status ?? 0;
        return new BatteryInfoData
        {
            HasBattery = true,
            ChargePercent = first.ChargePercent,
            PowerState = DescribeStatus(status),
            OnBattery = status == 1 || (status is 4 or 5),
            EstimatedRunTimeMinutes =
                first.RunTimeMinutes is { } minutes && minutes != UnknownRunTime && minutes > 0
                    ? minutes
                    : null
        };
    }

    /// <summary>Maps Win32_Battery.BatteryStatus codes to a friendly Chinese description.</summary>
    public static string DescribeStatus(ushort status)
    {
        return status switch
        {
            1 => "使用电池放电中",
            2 => "已接通交流电源",
            3 => "电量已充满",
            4 => "电量不足",
            5 => "电量严重不足",
            6 or 7 or 8 or 9 => "充电中",
            10 => "充电状态未知",
            11 => "部分充电",
            _ => $"未知状态 ({status})"
        };
    }

    private static IReadOnlyList<(ushort? Status, int? ChargePercent, int? RunTimeMinutes)> ReadFromWmi()
    {
        var list = new List<(ushort?, int?, int?)>();
        using var searcher = new ManagementObjectSearcher(
            "SELECT BatteryStatus, EstimatedChargeRemaining, EstimatedRunTime FROM Win32_Battery");
        foreach (var obj in searcher.Get())
        {
            using (obj)
            {
                list.Add((
                    TryToUShort(obj["BatteryStatus"]),
                    TryToInt(obj["EstimatedChargeRemaining"]),
                    TryToInt(obj["EstimatedRunTime"])));
            }
        }

        return list;
    }

    private static ushort? TryToUShort(object? value)
    {
        try
        {
            return value is null ? null : Convert.ToUInt16(value, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static int? TryToInt(object? value)
    {
        try
        {
            return value is null ? null : Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
