using System.Diagnostics;
using System.Management;
using DigYourWindows.Core.Models;

namespace DigYourWindows.Core.Services;

public interface ISystemInfoProvider
{
    double? GetSystemUptimeDays();
}

public class WmiSystemInfoProvider : ISystemInfoProvider
{
    private readonly ILogService _log;

    public WmiSystemInfoProvider(ILogService log)
    {
        _log = log;
    }

    public double? GetSystemUptimeDays()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem");
            foreach (var obj in searcher.Get())
            {
                using (obj)
                {
                    var lastBoot = ManagementDateTimeConverter.ToDateTime(obj["LastBootUpTime"]?.ToString() ?? string.Empty);
                    var uptime = DateTime.Now - lastBoot;
                    return uptime.TotalDays;
                }
            }
        }
        catch (Exception ex)
        {
            _log.Warn($"获取系统运行时间失败: {ex.Message}");
        }
        return null;
    }
}

public interface IPerformanceService
{
    PerformanceAnalysisData AnalyzeSystemPerformance(
        HardwareData hardware,
        List<LogEventData> events,
        List<ReliabilityRecordData> reliability);
}

public class PerformanceService : IPerformanceService
{
    private static readonly HashSet<uint> CriticalEventIds = new() { 41, 55, 57, 1001, 1003, 1073, 6008, 7034 };
    private static readonly HashSet<uint> BenignNoiseEventIds = new() { 10016, 219, 59, 10002, 7036 };

    private readonly ISystemInfoProvider _systemInfo;
    private readonly ILogService _log;

    public PerformanceService(ISystemInfoProvider systemInfo, ILogService log)
    {
        _systemInfo = systemInfo;
        _log = log;
    }

    public PerformanceAnalysisData AnalyzeSystemPerformance(
        HardwareData hardware,
        List<LogEventData> events,
        List<ReliabilityRecordData> reliability)
    {
        var recommendations = new List<string>();
        var eventAnalysis = AnalyzeEvents(events);
        var totalMemoryGB = hardware.TotalMemory / 1024d / 1024d / 1024d;
        var memoryUsageScore = CalculateMemoryScore(hardware, recommendations);
        var diskHealthScore = CalculateDiskScore(hardware, recommendations);
        var systemUptimeDays = _systemInfo.GetSystemUptimeDays();
        var stabilityScore = CalculateStabilityScore(
            eventAnalysis.ErrorCount,
            eventAnalysis.WarningCount,
            eventAnalysis.NoiseCount,
            eventAnalysis.CriticalEvents.Count,
            reliability.Count,
            recommendations);
        var performanceScore = CalculatePerformanceScore(
            hardware.CpuCores,
            hardware.CpuBrand,
            totalMemoryGB,
            recommendations);
        var systemHealthScore = CalculateSystemHealthScore(
            stabilityScore,
            performanceScore,
            memoryUsageScore,
            diskHealthScore);

        AddSummaryRecommendations(eventAnalysis.CriticalEvents.Count, systemHealthScore, recommendations);

        var (healthGrade, healthColor) = GetHealthGradeAndColor(systemHealthScore);

        return new PerformanceAnalysisData
        {
            SystemHealthScore = systemHealthScore,
            StabilityScore = stabilityScore,
            PerformanceScore = performanceScore,
            MemoryUsageScore = memoryUsageScore,
            DiskHealthScore = diskHealthScore,
            SystemUptimeDays = systemUptimeDays,
            CriticalIssuesCount = (uint)eventAnalysis.CriticalEvents.Count,
            WarningsCount = (uint)eventAnalysis.WarningCount,
            Recommendations = recommendations,
            HealthGrade = healthGrade,
            HealthColor = healthColor
        };
    }

    private static double CalculateMemoryScore(HardwareData hardware, List<string> recommendations)
    {
        var totalMemoryGB = hardware.TotalMemory / 1024d / 1024d / 1024d;

        // If available memory is recorded (live system)
        if (hardware.AvailableMemory > 0 && hardware.TotalMemory > 0)
        {
            var capacityScore = totalMemoryGB >= ScoringConfiguration.ExcellentMemoryThresholdGb ? 95d :
                                totalMemoryGB >= ScoringConfiguration.GoodMemoryThresholdGb ? 85d :
                                totalMemoryGB >= ScoringConfiguration.AcceptableMemoryThresholdGb ? 70d : 50d;

            var freePercentage = (double)hardware.AvailableMemory / hardware.TotalMemory * 100d;
            double utilizationScore;

            if (freePercentage >= 40d)
            {
                utilizationScore = 95d;
            }
            else if (freePercentage >= 25d)
            {
                utilizationScore = 85d;
            }
            else if (freePercentage >= 10d)
            {
                utilizationScore = 70d;
            }
            else
            {
                AddRecommendation(recommendations, $"系统可用内存偏低 ({freePercentage:F0}%)，建议关闭不必要的后台程序");
                utilizationScore = 40d;
            }

            if (totalMemoryGB < ScoringConfiguration.GoodMemoryThresholdGb)
            {
                AddRecommendation(recommendations, "内存容量较小，建议考虑升级到8GB或更多以提升性能");
            }

            return Math.Round(capacityScore * 0.4d + utilizationScore * 0.6d, 1);
        }

        // Backward-compatible fallback for unit tests and stubs
        if (totalMemoryGB >= ScoringConfiguration.ExcellentMemoryThresholdGb)
        {
            return ScoringConfiguration.ExcellentScore;
        }

        if (totalMemoryGB >= ScoringConfiguration.GoodMemoryThresholdGb)
        {
            return ScoringConfiguration.GoodScore;
        }

        if (totalMemoryGB >= ScoringConfiguration.AcceptableMemoryThresholdGb)
        {
            AddRecommendation(recommendations, "内存容量较小，建议考虑升级到8GB或更多以提升性能");
            return ScoringConfiguration.AcceptableScore;
        }

        AddRecommendation(recommendations, "内存容量严重不足，强烈建议升级到8GB或更多");
        return ScoringConfiguration.PoorScore;
    }

    private static double CalculateDiskScore(HardwareData hardware, List<string> recommendations)
    {
        var disks = hardware.Disks;
        if (disks.Count == 0)
        {
            AddRecommendation(recommendations, "未检测到磁盘信息，请检查磁盘连接");
            return 50d;
        }

        var totalScore = 0d;

        foreach (var disk in disks)
        {
            var freePercentage = disk.TotalSpace > 0
                ? (disk.AvailableSpace / (double)disk.TotalSpace) * 100d
                : 0d;

            totalScore += GetDiskScore(disk, freePercentage, recommendations);
        }

        var score = totalScore / disks.Count;

        // Check SMART health status if available
        if (hardware.DiskSmart.Count > 0)
        {
            foreach (var smart in hardware.DiskSmart)
            {
                if (smart.HealthStatus == 3)
                {
                    score -= ScoringConfiguration.SmartFailurePenalty;
                    AddRecommendation(recommendations, $"检测到磁盘 {smart.FriendlyName} 处于故障预警状态，请立即备份重要数据");
                }
                else if (smart.HealthStatus == 2)
                {
                    score -= ScoringConfiguration.SmartWarningPenalty;
                    AddRecommendation(recommendations, $"检测到磁盘 {smart.FriendlyName} 存在健康警告，建议排查 SMART 指标");
                }
            }
        }

        return Math.Clamp(score, 0d, 100d);
    }

    private static double GetDiskScore(DiskInfoData disk, double freePercentage, List<string> recommendations)
    {
        if (freePercentage > ScoringConfiguration.ExcellentDiskFreeThresholdPercent)
        {
            return ScoringConfiguration.ExcellentScore;
        }

        if (freePercentage > ScoringConfiguration.GoodDiskFreeThresholdPercent)
        {
            return ScoringConfiguration.GoodScore;
        }

        if (freePercentage > ScoringConfiguration.AcceptableDiskFreeThresholdPercent)
        {
            AddRecommendation(recommendations, $"磁盘 {disk.Name} 剩余空间不足 ({freePercentage:F0}%)，建议清理空间");
            return ScoringConfiguration.AcceptableScore;
        }

        AddRecommendation(recommendations, $"磁盘 {disk.Name} 剩余空间严重不足 ({freePercentage:F0}%)，请立即清理空间");
        return 30d;
    }

    private static double CalculateStabilityScore(
        int errorCount,
        int warningCount,
        int noiseCount,
        int criticalEventsCount,
        int reliabilityRecordsCount,
        List<string> recommendations)
    {
        var score = 100d;

        score -= Math.Min(ScoringConfiguration.MaxErrorPenalty, errorCount * ScoringConfiguration.ErrorPenaltyPerError);
        score -= Math.Min(ScoringConfiguration.MaxWarningPenalty, warningCount * ScoringConfiguration.WarningPenaltyPerWarning);
        score -= Math.Min(ScoringConfiguration.MaxNoisePenalty, noiseCount * ScoringConfiguration.NoisePenaltyPerEvent);
        score -= Math.Min(ScoringConfiguration.MaxCriticalEventPenalty, criticalEventsCount * ScoringConfiguration.CriticalEventPenalty);

        if (reliabilityRecordsCount > ScoringConfiguration.HighReliabilityRecordThreshold)
        {
            score -= ScoringConfiguration.HighReliabilityRecordPenalty;
            AddRecommendation(recommendations, "系统可靠性记录较多，建议检查系统稳定性");
        }

        return Math.Max(0d, score);
    }

    private static double CalculatePerformanceScore(
        uint cpuCount,
        string cpuBrand,
        double totalMemoryGB,
        List<string> recommendations)
    {
        var score = 50d;
        score += GetCpuCoreScore(cpuCount, recommendations);
        score += GetCpuBrandScore(cpuBrand);
        score += GetMemoryPerformanceScore(totalMemoryGB);

        return Math.Clamp(score, 0d, 100d);
    }

    private static double GetCpuCoreScore(uint cpuCount, List<string> recommendations)
    {
        if (cpuCount >= ScoringConfiguration.HighCoreCountThreshold)
        {
            return ScoringConfiguration.HighCoreCountScore;
        }

        if (cpuCount >= ScoringConfiguration.MediumCoreCountThreshold)
        {
            return ScoringConfiguration.MediumCoreCountScore;
        }

        if (cpuCount >= ScoringConfiguration.LowCoreCountThreshold)
        {
            return ScoringConfiguration.LowCoreCountScore;
        }

        AddRecommendation(recommendations, "CPU核心数较少，可能会影响多任务处理性能");
        return ScoringConfiguration.SingleCorePenalty;
    }

    private static double GetCpuBrandScore(string cpuBrand)
    {
        if (string.IsNullOrWhiteSpace(cpuBrand))
        {
            return 0d;
        }

        var brand = cpuBrand.ToLowerInvariant();

        // Intel detection - use word boundaries to avoid false matches
        if (brand.Contains("intel"))
        {
            if (ContainsWord(brand, "i9") || brand.Contains("xeon"))
            {
                return ScoringConfiguration.TopTierCpuScore;
            }

            if (ContainsWord(brand, "i7"))
            {
                return ScoringConfiguration.HighTierCpuScore;
            }

            if (ContainsWord(brand, "i5"))
            {
                return ScoringConfiguration.MidTierCpuScore;
            }

            if (ContainsWord(brand, "i3"))
            {
                return 0d;
            }

            // Check for generation number (e.g., i7-12700, i9-13900)
            var intelMatch = System.Text.RegularExpressions.Regex.Match(brand, @"i[3579][-\s]?(\d{1,2})\d{3}");
            if (intelMatch.Success && int.TryParse(intelMatch.Groups[1].Value, out var generation))
            {
                // Bonus for newer generations (12th gen and above)
                if (generation >= 12)
                {
                    return brand.Contains("i9") ? 18d : brand.Contains("i7") ? 12d : brand.Contains("i5") ? 7d : 0d;
                }
            }
        }
        else if (brand.Contains("amd"))
        {
            if (ContainsWord(brand, "ryzen 9") || brand.Contains("threadripper"))
            {
                return ScoringConfiguration.TopTierCpuScore;
            }

            if (ContainsWord(brand, "ryzen 7"))
            {
                return ScoringConfiguration.HighTierCpuScore;
            }

            if (ContainsWord(brand, "ryzen 5"))
            {
                return ScoringConfiguration.MidTierCpuScore;
            }

            if (ContainsWord(brand, "ryzen 3"))
            {
                return 0d;
            }

            // Check for Ryzen series number (e.g., Ryzen 9 7950X)
            var amdMatch = System.Text.RegularExpressions.Regex.Match(brand, @"ryzen\s*\d\s*(\d{1,2})\d{3}");
            if (amdMatch.Success && int.TryParse(amdMatch.Groups[1].Value, out var series))
            {
                // Bonus for 7000/9000 series (Zen 4/5)
                if (series >= 7)
                {
                    return brand.Contains("ryzen 9") ? 18d : brand.Contains("ryzen 7") ? 12d : brand.Contains("ryzen 5") ? 7d : 0d;
                }
            }

            // EPYC server processors
            if (brand.Contains("epyc"))
            {
                return ScoringConfiguration.TopTierCpuScore;
            }
        }

        // Apple Silicon detection (for future compatibility)
        if (brand.Contains("apple") || brand.Contains("m1") || brand.Contains("m2") || brand.Contains("m3"))
        {
            if (brand.Contains("max") || brand.Contains("ultra"))
            {
                return ScoringConfiguration.TopTierCpuScore;
            }

            if (brand.Contains("pro"))
            {
                return ScoringConfiguration.HighTierCpuScore;
            }

            return ScoringConfiguration.MidTierCpuScore;
        }

        return 0d;
    }

    private static bool ContainsWord(string text, string word)
    {
        var index = text.IndexOf(word, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return false;
        }

        // Check that it's a word boundary (not part of another word)
        var beforeIsBoundary = index == 0 || !char.IsLetterOrDigit(text[index - 1]);
        var afterIndex = index + word.Length;
        var afterIsBoundary = afterIndex >= text.Length || !char.IsLetterOrDigit(text[afterIndex]);

        return beforeIsBoundary && afterIsBoundary;
    }

    private static double GetMemoryPerformanceScore(double totalMemoryGB)
    {
        if (totalMemoryGB >= ScoringConfiguration.ExcellentMemoryThresholdGb)
        {
            return 15d;
        }

        if (totalMemoryGB >= ScoringConfiguration.GoodMemoryThresholdGb)
        {
            return 10d;
        }

        if (totalMemoryGB >= ScoringConfiguration.AcceptableMemoryThresholdGb)
        {
            return 5d;
        }

        return -5d;
    }

    private static EventAnalysisResult AnalyzeEvents(List<LogEventData> events)
    {
        var errorCount = 0;
        var warningCount = 0;
        var noiseCount = 0;
        var criticalEvents = new List<LogEventData>();

        foreach (var evt in events)
        {
            var eventType = evt.EventType.ToLowerInvariant();
            if (eventType == "error")
            {
                if (IsCriticalError(evt))
                {
                    errorCount++;
                    criticalEvents.Add(evt);
                }
                else if (IsBenignNoise(evt))
                {
                    noiseCount++;
                }
                else
                {
                    errorCount++;
                }
            }
            else if (eventType == "warning")
            {
                if (IsBenignNoise(evt))
                {
                    noiseCount++;
                }
                else
                {
                    warningCount++;
                }
            }
        }

        return new EventAnalysisResult
        {
            ErrorCount = errorCount,
            WarningCount = warningCount,
            NoiseCount = noiseCount,
            CriticalEvents = criticalEvents
        };
    }

    private static bool IsBenignNoise(LogEventData evt)
    {
        if (BenignNoiseEventIds.Contains(evt.EventId))
        {
            return true;
        }

        var src = evt.SourceName;
        if (src.Contains("DistributedCOM", StringComparison.OrdinalIgnoreCase) ||
            src.Contains("SideBySide", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static bool IsCriticalError(LogEventData evt)
    {
        return CriticalEventIds.Contains(evt.EventId) ||
               evt.SourceName.Contains("bugcheck", StringComparison.OrdinalIgnoreCase) ||
               evt.Message.Contains("critical", StringComparison.OrdinalIgnoreCase) ||
               evt.Message.Contains("fatal", StringComparison.OrdinalIgnoreCase);
    }

    private static double CalculateSystemHealthScore(
        double stabilityScore,
        double performanceScore,
        double memoryUsageScore,
        double diskHealthScore)
    {
        var score = stabilityScore * ScoringConfiguration.StabilityWeight +
                    performanceScore * ScoringConfiguration.PerformanceWeight +
                    memoryUsageScore * ScoringConfiguration.MemoryWeight +
                    diskHealthScore * ScoringConfiguration.DiskWeight;

        return Math.Clamp(score, 0d, 100d);
    }

    private static void AddSummaryRecommendations(int criticalEventCount, double systemHealthScore, List<string> recommendations)
    {
        if (criticalEventCount > 0)
        {
            AddRecommendation(recommendations, $"发现 {criticalEventCount} 个严重系统错误，建议立即检查系统日志");
        }

        if (systemHealthScore < ScoringConfiguration.AcceptableHealthThreshold)
        {
            AddRecommendation(recommendations, "系统健康评分较低，建议进行全面系统维护");
        }
    }

    private static void AddRecommendation(List<string> recommendations, string message)
    {
        if (!recommendations.Contains(message, StringComparer.Ordinal))
        {
            recommendations.Add(message);
        }
    }

    private sealed record EventAnalysisResult
    {
        public int ErrorCount { get; init; }
        public int WarningCount { get; init; }
        public int NoiseCount { get; init; }
        public List<LogEventData> CriticalEvents { get; init; } = new();
    }

    private static (string Grade, string Color) GetHealthGradeAndColor(double systemHealthScore)
    {
        return systemHealthScore switch
        {
            >= ScoringConfiguration.ExcellentHealthThreshold => (ScoringConfiguration.ExcellentHealthGrade, ScoringConfiguration.ExcellentHealthColor),
            >= ScoringConfiguration.GoodHealthThreshold => (ScoringConfiguration.GoodHealthGrade, ScoringConfiguration.GoodHealthColor),
            >= ScoringConfiguration.AcceptableHealthThreshold => (ScoringConfiguration.AcceptableHealthGrade, ScoringConfiguration.AcceptableHealthColor),
            >= ScoringConfiguration.PoorHealthThreshold => (ScoringConfiguration.PoorHealthGrade, ScoringConfiguration.PoorHealthColor),
            _ => (ScoringConfiguration.CriticalHealthGrade, ScoringConfiguration.CriticalHealthColor)
        };
    }
}
