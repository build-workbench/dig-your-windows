using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using DigYourWindows.UI.Services;
using DigYourWindows.UI.ViewModels;
using Wpf.Ui.Controls;

namespace DigYourWindows.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    private readonly IAppSettingsService _settings;
    private readonly DigYourWindows.Core.Services.ILogService _log;

    public MainWindow(
        MainViewModel viewModel,
        IAppSettingsService settings,
        Wpf.Ui.Abstractions.INavigationViewPageProvider pageService,
        DigYourWindows.Core.Services.ILogService log)
    {
        _log = log;
        InitializeComponent();
        DataContext = viewModel;
        _settings = settings;

        RootNavigation.SetPageProviderService(pageService);
        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;

        ApplySettings(_settings.Current);
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // WPF-UI TitleBar 在 ContentRendered 时注册了返回 HTCAPTION 的钩子，会抢先吞掉窗口顶部
        // 边缘的 resize 命中测试（HwndSource 钩子按后注册先调用），导致左上角/顶部无法调整窗口大小。
        // 因此延迟到 ContentRendered 之后（ApplicationIdle 优先级）再注册我们的边缘命中测试钩子。
        _ = Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(RegisterResizeHitTestHook));

        try
        {
            // Automatically adapt to and watch Windows system theme changes
            Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

            RootNavigation.Navigate(typeof(Views.DashboardPage));

            if (DataContext is MainViewModel viewModel)
            {
                // Synchronize initial theme with current Windows system setting
                viewModel.ApplySystemTheme();

                // Initialize history on startup
                await viewModel.InitializeHistoryAsync();

                // Load current diagnostic data
                await viewModel.LoadDataCommand.ExecuteAsync(null);

                // Load history list
                if (viewModel.HistoryListViewModel != null)
                {
                    await viewModel.HistoryListViewModel.LoadHistoryCommand.ExecuteAsync(null);
                }
            }
        }
        catch (Exception ex)
        {
            _log.LogError($"加载数据失败: {ex.Message}", ex);
            System.Windows.MessageBox.Show($"加载数据失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Registers the resize hit-test hook after WPF-UI's TitleBar has registered its own
    /// (which returns HTCAPTION for the whole title bar area and would otherwise swallow
    /// the top edge resize hit tests).
    /// </summary>
    private void RegisterResizeHitTestHook()
    {
        var handle = new WindowInteropHelper(this).Handle;
        var source = HwndSource.FromHwnd(handle);
        source?.AddHook(WndProc);
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;
        if (handle != IntPtr.Zero)
        {
            var source = HwndSource.FromHwnd(handle);
            source?.RemoveHook(WndProc);
        }

        Wpf.Ui.Appearance.SystemThemeWatcher.UnWatch(this);
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private const int WM_NCHITTEST = 0x0084;
    private const int HTLEFT = 10;
    private const int HTRIGHT = 11;
    private const int HTTOP = 12;
    private const int HTTOPLEFT = 13;
    private const int HTTOPRIGHT = 14;
    private const int HTBOTTOM = 15;
    private const int HTBOTTOMLEFT = 16;
    private const int HTBOTTOMRIGHT = 17;

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_NCHITTEST && ResizeMode != ResizeMode.NoResize && WindowState == WindowState.Normal)
        {
            short x = unchecked((short)(long)lParam);
            short y = unchecked((short)((long)lParam >> 16));

            if (GetWindowRect(hwnd, out RECT rc))
            {
                var dpi = VisualTreeHelper.GetDpi(this);
                int border = Math.Max(6, (int)Math.Round(8 * dpi.PixelsPerDip));

                bool isLeftEdge = x >= rc.Left && x < rc.Left + border;
                bool isRightEdge = x <= rc.Right && x > rc.Right - border;
                bool isTopEdge = y >= rc.Top && y < rc.Top + border;
                bool isBottomEdge = y <= rc.Bottom && y > rc.Bottom - border;

                if (isTopEdge && isLeftEdge)
                {
                    handled = true;
                    return (IntPtr)HTTOPLEFT;
                }
                if (isTopEdge && isRightEdge)
                {
                    handled = true;
                    return (IntPtr)HTTOPRIGHT;
                }
                if (isBottomEdge && isLeftEdge)
                {
                    handled = true;
                    return (IntPtr)HTBOTTOMLEFT;
                }
                if (isBottomEdge && isRightEdge)
                {
                    handled = true;
                    return (IntPtr)HTBOTTOMRIGHT;
                }
                if (isLeftEdge)
                {
                    handled = true;
                    return (IntPtr)HTLEFT;
                }
                if (isRightEdge)
                {
                    handled = true;
                    return (IntPtr)HTRIGHT;
                }
                if (isBottomEdge)
                {
                    handled = true;
                    return (IntPtr)HTBOTTOM;
                }
                if (isTopEdge)
                {
                    handled = true;
                    return (IntPtr)HTTOP;
                }
            }
        }

        return IntPtr.Zero;
    }

    private void OpenSettingsClicked(object sender, RoutedEventArgs e)
    {
        var dialog = new SettingsWindow(_settings);
        dialog.Owner = this;
        if (dialog.ShowDialog() == true && dialog.Result is { } settings)
        {
            ApplySettings(settings);
        }
    }

    private void ApplySettings(AppSettings settings)
    {
        var scale = settings.ScalePercent / 100d;
        ContentScale.ScaleX = scale;
        ContentScale.ScaleY = scale;

        if (!string.IsNullOrWhiteSpace(settings.FontFamily))
        {
            FontFamily = new FontFamily(settings.FontFamily);
        }
    }
}
