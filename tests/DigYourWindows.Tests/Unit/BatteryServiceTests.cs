using DigYourWindows.Core.Services;

namespace DigYourWindows.Tests.Unit;

/// <summary>
/// Unit tests for BatteryService state mapping and missing-battery fallback.
/// </summary>
public class BatteryServiceTests
{
    private sealed class LogSink : ILogService
    {
        public void Info(string message) { }
        public void Warn(string message) { }
        public void LogError(string message, Exception? exception = null) { }
    }

    private static IReadOnlyList<(ushort? Status, int? ChargePercent, int? RunTimeMinutes)> Battery(
        ushort status, int charge, int runtimeMinutes) =>
        [(status, charge, runtimeMinutes)];

    [Fact]
    public void GetBatteryInfo_WithAcPowerAndCharge_MapsAllFields()
    {
        var service = new BatteryService(new LogSink(), () => Battery(2, 80, 120));

        var info = service.GetBatteryInfo();

        Assert.True(info.HasBattery);
        Assert.Equal(80, info.ChargePercent);
        Assert.Equal("已接通交流电源", info.PowerState);
        Assert.False(info.OnBattery);
        Assert.Equal(120, info.EstimatedRunTimeMinutes);
    }

    [Fact]
    public void GetBatteryInfo_WhileDischarging_MarksOnBattery()
    {
        var service = new BatteryService(new LogSink(), () => Battery(1, 40, 60));

        var info = service.GetBatteryInfo();

        Assert.True(info.OnBattery);
        Assert.Equal("使用电池放电中", info.PowerState);
    }

    [Fact]
    public void GetBatteryInfo_ChargingStates_MapToCharging()
    {
        foreach (var status in new ushort[] { 6, 7, 8, 9 })
        {
            var service = new BatteryService(new LogSink(), () => Battery(status, 50, 30));
            Assert.Equal("充电中", service.GetBatteryInfo().PowerState);
        }
    }

    [Fact]
    public void GetBatteryInfo_WithNoBattery_ReturnsNotDetected()
    {
        var service = new BatteryService(new LogSink(), () => Array.Empty<(ushort?, int?, int?)>());

        var info = service.GetBatteryInfo();

        Assert.False(info.HasBattery);
        Assert.Equal("未检测到电池", info.PowerState);
        Assert.Null(info.ChargePercent);
    }

    [Fact]
    public void GetBatteryInfo_WithUnknownRuntimeSentinel_ReturnsNull()
    {
        // Win32_Battery reports 71582788 when the runtime estimate is unknown
        var service = new BatteryService(new LogSink(), () => Battery(2, 80, 71582788));

        Assert.Null(service.GetBatteryInfo().EstimatedRunTimeMinutes);
    }

    [Fact]
    public void GetBatteryInfo_WhenSourceThrows_ReturnsNotDetected()
    {
        var service = new BatteryService(new LogSink(), () => throw new System.Management.ManagementException());

        var info = service.GetBatteryInfo();

        Assert.False(info.HasBattery);
    }
}
