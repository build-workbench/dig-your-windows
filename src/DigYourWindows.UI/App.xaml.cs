using System.IO;
using System.Windows;
using DigYourWindows.Core.Services;
using DigYourWindows.UI.Services;
using DigYourWindows.UI.ViewModels;
using DigYourWindows.UI.Views.Pages;
using DigYourWindows.UI.Views.Windows;
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

        // Synchronize application theme with Windows OS system theme
        Wpf.Ui.Appearance.ApplicationThemeManager.ApplySystemTheme();

        var services = new ServiceCollection();
        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();

        // History store must be initialized before anything touches it.
        // Per IHistoryStoreService contract, failures disable history features
        // but must not prevent app startup.
        var log = _serviceProvider.GetRequiredService<ILogService>();
        try
        {
            await _serviceProvider.GetRequiredService<IHistoryStoreService>().InitializeAsync();
            log.Info("History store ready.");
        }
        catch (Exception ex)
        {
            log.LogError(
                $"History store initialization failed; history features disabled: {ex.Message}", ex);
        }

        try
        {
            log.Info("Resolving MainWindow...");
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            log.Info("Showing MainWindow...");
            mainWindow.Show();
            log.Info("MainWindow shown.");

            if (e.Args.Contains("--capture-preview"))
            {
                var outputDir = e.Args.SkipWhile(a => a != "--capture-preview").Skip(1).FirstOrDefault()
                                ?? Path.Combine(AppContext.BaseDirectory, "previews");
                _ = CapturePreviewsAsync(mainWindow, outputDir, log);
            }
        }
        catch (Exception ex)
        {
            log.LogError($"MainWindow creation failed: {ex.Message}", ex);
            throw;
        }
    }

    private async Task CapturePreviewsAsync(MainWindow mainWindow, string outputDir, ILogService log)
    {
        try
        {
            // Wait for initial data collection and UI rendering
            await Task.Delay(4000);

            await Dispatcher.InvokeAsync(async () =>
            {
                Directory.CreateDirectory(outputDir);

                SaveVisualSnapshot(mainWindow.Content as FrameworkElement ?? mainWindow, Path.Combine(outputDir, "dashboard_preview.png"));
                log.Info("Captured dashboard_preview.png");

                // Switch to MonitoringPage
                mainWindow.RootNavigation.Navigate(typeof(MonitoringPage));
                await Task.Delay(1000);
                SaveVisualSnapshot(mainWindow.Content as FrameworkElement ?? mainWindow, Path.Combine(outputDir, "monitoring_preview.png"));
                log.Info("Captured monitoring_preview.png");

                // Switch to LogsPage
                mainWindow.RootNavigation.Navigate(typeof(LogsPage));
                await Task.Delay(1000);
                SaveVisualSnapshot(mainWindow.Content as FrameworkElement ?? mainWindow, Path.Combine(outputDir, "logs_preview.png"));
                log.Info("Captured logs_preview.png");

                // Switch to HardwarePage
                mainWindow.RootNavigation.Navigate(typeof(HardwarePage));
                await Task.Delay(1000);
                SaveVisualSnapshot(mainWindow.Content as FrameworkElement ?? mainWindow, Path.Combine(outputDir, "hardware_preview.png"));
                log.Info("Captured hardware_preview.png");

                log.Info("All previews captured successfully. Shutting down.");
                Shutdown();
            });
        }
        catch (Exception ex)
        {
            log.LogError($"Failed to capture visual previews: {ex.Message}", ex);
            Shutdown();
        }
    }

    private static void SaveVisualSnapshot(FrameworkElement element, string filePath)
    {
        element.UpdateLayout();
        var width = (int)Math.Max(element.ActualWidth > 0 ? element.ActualWidth : 1400, 1400);
        var height = (int)Math.Max(element.ActualHeight > 0 ? element.ActualHeight : 900, 900);
        var rtb = new System.Windows.Media.Imaging.RenderTargetBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
        rtb.Render(element);
        var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
        encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(rtb));
        using var stream = File.Create(filePath);
        encoder.Save(stream);
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
        services.AddSingleton<IAppSettingsService, AppSettingsService>();
        services.AddSingleton<ViewModels.HistoryListViewModel>();

        // Navigation page provider and page views
        services.AddSingleton<Wpf.Ui.Abstractions.INavigationViewPageProvider, PageService>();
        services.AddSingleton<DashboardPage>();
        services.AddSingleton<MonitoringPage>();
        services.AddSingleton<LogsPage>();
        services.AddSingleton<HardwarePage>();
        services.AddSingleton<HistoryPage>();
    }
}
