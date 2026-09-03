using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DigYourWindows.Core.Models;
using DigYourWindows.Core.Services;
using DigYourWindows.UI.Services;
using ScottPlot.WPF;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Threading;
using Wpf.Ui.Appearance;

namespace DigYourWindows.UI.ViewModels;

/// <summary>
/// Orchestrates diagnostic data flow: collection, import/export, real-time
/// monitoring state and history. Rendering, dialogs and theme application are
/// delegated to injected UI services.
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IDiagnosticCollectorService _collectorService;
    private readonly IReportService _reportService;
    private readonly ICpuMonitorService _cpuMonitorService;
    private readonly INetworkMonitorService _networkMonitorService;
    private readonly IHistoryStoreService _historyStoreService;
    private readonly ILogService _log;
    private readonly IMonitorPlotService _plots;
    private readonly IApplicationThemeService _themeService;
    private readonly IFileDialogService _dialogs;
    private readonly DispatcherTimer _cpuMonitorTimer;
    private CancellationTokenSource? _loadCts;
    private DiagnosticData? _currentData;
    private bool _reloadRequested;

    [ObservableProperty]
    private HardwareData? _hardwareInfo;

    [ObservableProperty]
    private CpuInfoData _cpuInfo = new();

    [ObservableProperty]
    private double _networkDownloadMBps;

    [ObservableProperty]
    private double _networkUploadMBps;

    [ObservableProperty]
    private ObservableCollection<ReliabilityRecordData> _reliabilityRecords = new();

    [ObservableProperty]
    private ObservableCollection<LogEventData> _eventLogEntries = new();

    [ObservableProperty]
    private PerformanceAnalysisData? _performanceAnalysis;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "就绪";

    [ObservableProperty]
    private int _selectedDaysBack = 3;

    [ObservableProperty]
    private ApplicationTheme _currentTheme = ApplicationTheme.Dark;

    [ObservableProperty]
    private DiagnosticHistorySummary? _recentHistoryEntry;

    [ObservableProperty]
    private HistoryListViewModel? _historyListViewModel;

    public List<int> AvailableDays { get; } = new() { 1, 3, 7, 30 };

    public WpfPlot ReliabilityTrendPlot => _plots.ReliabilityTrendPlot;

    public WpfPlot NetworkTrafficPlot => _plots.NetworkTrafficPlot;

    public MainViewModel(
        IDiagnosticCollectorService collectorService,
        IReportService reportService,
        ICpuMonitorService cpuMonitorService,
        INetworkMonitorService networkMonitorService,
        IHistoryStoreService historyStoreService,
        ILogService log,
        IMonitorPlotService plots,
        IApplicationThemeService themeService,
        IFileDialogService dialogs,
        HistoryListViewModel historyListViewModel)
    {
        _collectorService = collectorService;
        _reportService = reportService;
        _cpuMonitorService = cpuMonitorService;
        _networkMonitorService = networkMonitorService;
        _historyStoreService = historyStoreService;
        _log = log;
        _plots = plots;
        _themeService = themeService;
        _dialogs = dialogs;
        HistoryListViewModel = historyListViewModel;
        historyListViewModel.EntrySelected += OnHistoryEntrySelected;

        _cpuMonitorTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _cpuMonitorTimer.Tick += CpuMonitorTimer_Tick;
        _cpuMonitorTimer.Start();

        UpdateCpuInfo();
        UpdateNetworkTraffic();
        UpdateReliabilityTrendPlot();
        UpdateNetworkTrafficPlot();
    }

    private void CpuMonitorTimer_Tick(object? sender, EventArgs e)
    {
        UpdateCpuInfo();
        UpdateNetworkTraffic();
    }

    private void UpdateCpuInfo()
    {
        CpuInfo = _cpuMonitorService.GetCpuInfo();
    }

    private void UpdateNetworkTraffic()
    {
        _networkMonitorService.Update();
        NetworkDownloadMBps = _networkMonitorService.DownloadMBps;
        NetworkUploadMBps = _networkMonitorService.UploadMBps;
        UpdateNetworkTrafficPlot();
    }

    private void UpdateNetworkTrafficPlot()
    {
        _plots.RenderNetworkTraffic(
            _networkMonitorService.HistoryTimes,
            _networkMonitorService.HistoryDownload,
            _networkMonitorService.HistoryUpload,
            IsDarkTheme);
    }

    private void UpdateReliabilityTrendPlot()
    {
        if (ReliabilityRecords.Count == 0)
        {
            _plots.RenderReliabilityTrend(Array.Empty<ReliabilityTrendDayCounts>(), IsDarkTheme);
            return;
        }

        var trend = ReliabilityTrendBuilder.BuildDailyCounts(ReliabilityRecords, SelectedDaysBack);
        _plots.RenderReliabilityTrend(trend, IsDarkTheme);
    }

    private bool IsDarkTheme => CurrentTheme == ApplicationTheme.Dark;

    public void Dispose()
    {
        _cpuMonitorTimer.Stop();
        _cpuMonitorTimer.Tick -= CpuMonitorTimer_Tick;
        if (HistoryListViewModel is not null)
        {
            HistoryListViewModel.EntrySelected -= OnHistoryEntrySelected;
        }
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        // WpfPlot does not implement IDisposable, so no explicit disposal needed
        GC.SuppressFinalize(this);
    }

    partial void OnSelectedDaysBackChanged(int value)
    {
        if (IsLoading)
        {
            _reloadRequested = true;
            return;
        }

        _ = LoadDataAsyncSafe();
    }

    private async Task LoadDataAsyncSafe()
    {
        try
        {
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            _log.LogError("加载数据发生意外错误", ex);
        }
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;
        StatusMessage = "正在加载数据...";

        _loadCts?.Cancel();
        _loadCts = new CancellationTokenSource();

        try
        {
            var progress = new Progress<DiagnosticCollectionProgress>(p =>
            {
                StatusMessage = p.Message;
            });

            var daysBack = SelectedDaysBack;
            var result = await _collectorService.CollectAsync(daysBack, progress, _loadCts.Token);
            ApplyDiagnosticData(result.Data);

            if (result.Warnings.Count > 0)
            {
                _log.Warn($"数据采集存在 {result.Warnings.Count} 条警告: {string.Join(" | ", result.Warnings)}");
            }

            StatusMessage = BuildLoadCompletedStatus(result);
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "已取消";
        }
        catch (Exception ex)
        {
            _log.LogError("加载失败", ex);
            StatusMessage = $"加载失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;

            if (_reloadRequested)
            {
                _reloadRequested = false;
                _ = LoadDataAsync();
            }
        }
    }

    [RelayCommand]
    private async Task ImportFromJsonAsync()
    {
        var fileName = _dialogs.PickJsonFileToOpen();
        if (string.IsNullOrEmpty(fileName))
        {
            return;
        }

        try
        {
            StatusMessage = "正在导入JSON报告...";
            IsLoading = true;

            var json = await Task.Run(() => File.ReadAllText(fileName, Encoding.UTF8));
            var data = _reportService.DeserializeFromJson(json);

            if (data == null)
            {
                StatusMessage = "导入失败: JSON 解析结果为空";
                return;
            }

            ApplyDiagnosticData(data);
            StatusMessage = BuildImportCompletedStatus(data);
        }
        catch (Exception ex)
        {
            _log.LogError("导入失败", ex);
            StatusMessage = $"导入失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyDiagnosticData(DiagnosticData data)
    {
        _currentData = data;
        HardwareInfo = data.Hardware;
        ReplaceCollection(ReliabilityRecords, data.Reliability);
        ReplaceCollection(EventLogEntries, data.Events);
        PerformanceAnalysis = data.Performance;
        UpdateReliabilityTrendPlot();
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    private DiagnosticData? BuildDiagnosticDataForExport()
    {
        if (_currentData is not null && HasMeaningfulData(_currentData))
        {
            return _currentData;
        }

        var data = new DiagnosticData
        {
            Hardware = HardwareInfo ?? new HardwareData(),
            Reliability = ReliabilityRecords.ToList(),
            Events = EventLogEntries.ToList(),
            Performance = PerformanceAnalysis ?? new PerformanceAnalysisData(),
            CollectedAt = _currentData?.CollectedAt ?? DateTime.UtcNow
        };

        if (!HasMeaningfulData(data))
        {
            return null;
        }

        _currentData = data;
        return data;
    }

    private static bool HasMeaningfulData(DiagnosticData data)
    {
        return !string.IsNullOrWhiteSpace(data.Hardware.ComputerName) ||
               !string.IsNullOrWhiteSpace(data.Hardware.OsVersion) ||
               !string.IsNullOrWhiteSpace(data.Hardware.CpuBrand) ||
               data.Hardware.TotalMemory > 0 ||
               data.Hardware.Disks.Count > 0 ||
               data.Hardware.Gpus.Count > 0 ||
               data.Reliability.Count > 0 ||
               data.Events.Count > 0 ||
               data.Performance.SystemHealthScore > 0 ||
               data.Performance.StabilityScore > 0 ||
               data.Performance.PerformanceScore > 0 ||
               data.Performance.MemoryUsageScore > 0 ||
               data.Performance.DiskHealthScore > 0 ||
               data.Performance.CriticalIssuesCount > 0 ||
               data.Performance.WarningsCount > 0 ||
               data.Performance.Recommendations.Count > 0;
    }

    [RelayCommand]
    private Task ExportToJsonAsync()
    {
        return ExportReportAsync(
            loadingMessage: "正在导出JSON报告...",
            extension: "json",
            successPrefix: "JSON已导出",
            contentFactory: data => _reportService.SerializeToJson(data, indented: true));
    }

    [RelayCommand]
    private Task ExportToHtmlAsync()
    {
        return ExportReportAsync(
            loadingMessage: "正在导出HTML报告...",
            extension: "html",
            successPrefix: "报告已导出",
            contentFactory: data => _reportService.GenerateHtmlReport(data, SelectedDaysBack));
    }

    private async Task ExportReportAsync(
        string loadingMessage,
        string extension,
        string successPrefix,
        Func<DiagnosticData, string> contentFactory)
    {
        try
        {
            StatusMessage = loadingMessage;
            IsLoading = true;

            var data = BuildDiagnosticDataForExport();
            if (data is null)
            {
                StatusMessage = "导出失败: 当前没有可导出的诊断数据";
                return;
            }

            var content = contentFactory(data);
            var (fileName, filePath) = BuildExportPath(extension);
            await WriteExportFileAsync(filePath, content);

            StatusMessage = BuildExportSuccessStatus(successPrefix, fileName);
            _dialogs.RevealFile(filePath);
        }
        catch (Exception ex)
        {
            _log.LogError($"导出{extension.ToUpperInvariant()}失败", ex);
            StatusMessage = $"导出失败: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private static (string FileName, string FilePath) BuildExportPath(string extension)
    {
        var fileName = $"DigYourWindows_Report_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";
        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
        return (fileName, filePath);
    }

    private static Task WriteExportFileAsync(string filePath, string content)
    {
        return Task.Run(() => File.WriteAllText(filePath, content, Encoding.UTF8));
    }

    private static string BuildLoadCompletedStatus(DiagnosticCollectionResult result)
    {
        var performanceScore = result.Data.Performance.SystemHealthScore;
        return $"数据加载完成 | 可靠性记录: {result.Data.Reliability.Count} | 错误事件: {result.Data.Events.Count} | 系统健康评分: {performanceScore:F0}/100" +
               (result.Warnings.Count > 0 ? $" | 警告: {result.Warnings.Count}" : string.Empty);
    }

    private static string BuildImportCompletedStatus(DiagnosticData data)
    {
        return $"JSON已导入 | 采集时间(UTC): {data.CollectedAt:yyyy-MM-dd HH:mm:ss} | 可靠性记录: {data.Reliability.Count} | 错误事件: {data.Events.Count}";
    }

    private static string BuildExportSuccessStatus(string successPrefix, string fileName)
    {
        return $"{successPrefix}: {fileName}";
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        CurrentTheme = _themeService.ToggleTheme();
        UpdateReliabilityTrendPlot();
        UpdateNetworkTrafficPlot();
        StatusMessage = BuildThemeChangedStatus();
    }

    private string BuildThemeChangedStatus()
    {
        return $"主题已切换为: {(CurrentTheme == ApplicationTheme.Dark ? "深色" : "浅色")}";
    }

    private void OnHistoryEntrySelected(DiagnosticHistorySummary entry)
    {
        ReloadHistoryEntryCommand.Execute(entry.Id);
    }

    /// <summary>
    /// Initialize history loading on app startup.
    /// Called from MainWindow after the view is loaded.
    /// </summary>
    public async Task InitializeHistoryAsync()
    {
        try
        {
            RecentHistoryEntry = await _historyStoreService.GetMostRecentSummaryAsync();
        }
        catch (Exception ex)
        {
            _log.LogError($"Failed to initialize history in ViewModel: {ex.Message}", ex);
        }
    }

    [RelayCommand]
    public async Task ReloadHistoryEntryAsync(string? historyId)
    {
        if (string.IsNullOrEmpty(historyId))
            return;

        try
        {
            var record = await _historyStoreService.LoadByIdAsync(historyId);
            if (record != null)
            {
                // Apply the loaded diagnostic data to the current display
                ApplyDiagnosticData(record.DiagnosticData);
                StatusMessage = $"加载历史诊断: {record.Summary.CollectedAtUtc:G}";
            }
        }
        catch (Exception ex)
        {
            _log.LogError($"Failed to reload history entry {historyId}: {ex.Message}", ex);
            StatusMessage = $"加载历史失败: {ex.Message}";
        }
    }
}
