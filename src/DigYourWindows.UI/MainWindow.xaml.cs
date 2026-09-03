using System.Windows;
using System.Windows.Media;
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

    public MainWindow(MainViewModel viewModel, IAppSettingsService settings)
    {
        InitializeComponent();
        DataContext = viewModel;
        _settings = settings;
        Loaded += MainWindow_Loaded;
        Closed += MainWindow_Closed;

        ApplySettings(_settings.Current);
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is MainViewModel viewModel)
            {
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
            System.Windows.MessageBox.Show($"加载数据失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MainWindow_Closed(object? sender, EventArgs e)
    {
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
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
