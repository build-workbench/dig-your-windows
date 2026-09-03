using System.IO;
using System.Windows;
using DigYourWindows.Core.Services;
using DigYourWindows.UI.Services;
using DigYourWindows.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DigYourWindows.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    public IServiceProvider Services => _serviceProvider ?? throw new InvalidOperationException("Service provider not initialized.");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();

        // History store must be initialized before anything touches it.
        // Per IHistoryStoreService contract, failures disable history features
        // but must not prevent app startup.
        try
        {
            var log = _serviceProvider.GetRequiredService<ILogService>();
            await _serviceProvider.GetRequiredService<IHistoryStoreService>().InitializeAsync();
            log.Info("History store ready.");
        }
        catch (Exception ex)
        {
            _serviceProvider.GetRequiredService<ILogService>().LogError(
                $"History store initialization failed; history features disabled: {ex.Message}", ex);
        }

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // History store (SQLite-backed persistence)
        services.AddSingleton<IHistoryStoreService>(sp =>
        {
            var historyDbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DigYourWindows",
                "history.db");
            Directory.CreateDirectory(Path.GetDirectoryName(historyDbPath) ?? "");
            return new SqliteHistoryStoreService(historyDbPath, sp.GetRequiredService<ILogService>());
        });

        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainViewModel>();

        services.AddSingleton<ILogService, FileLogService>();
        services.AddSingleton<IReportService, ReportService>();
        services.AddSingleton<IDiagnosticCollectorService, DiagnosticCollectorService>();

        services.AddSingleton<IHardwareMonitorProvider, HardwareMonitorProvider>();
        services.AddSingleton<ICpuMonitorService, CpuMonitorService>();
        services.AddSingleton<INetworkMonitorService, NetworkMonitorService>();
        services.AddSingleton<IGpuMonitorService, GpuMonitorService>();
        services.AddSingleton<IDiskSmartService, DiskSmartService>();
        services.AddSingleton<IHardwareService, HardwareService>();
        services.AddSingleton<IReliabilityService, ReliabilityService>();
        services.AddSingleton<IEventLogService, EventLogService>();
        services.AddSingleton<ISystemInfoProvider, WmiSystemInfoProvider>();
        services.AddSingleton<IPerformanceService, PerformanceService>();

        // UI concerns wrapped as services so ViewModels stay framework-agnostic
        services.AddSingleton<IMonitorPlotService, MonitorPlotService>();
        services.AddSingleton<IApplicationThemeService, ApplicationThemeService>();
        services.AddSingleton<IFileDialogService, FileDialogService>();
        services.AddSingleton<ViewModels.HistoryListViewModel>();
    }
}
