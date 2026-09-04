using System.Collections.Generic;
using System.Windows;
using DigYourWindows.UI.Services;
using Wpf.Ui.Controls;

namespace DigYourWindows.UI.Views.Windows;

/// <summary>
/// Settings dialog: font family and UI scale.
/// Returns the chosen settings via <see cref="Result"/> when confirmed.
/// </summary>
public partial class SettingsWindow : FluentWindow
{
    private static readonly IReadOnlyList<FontOption> Fonts =
    [
        new("微软雅黑", "Microsoft YaHei UI"),
        new("等线", "DengXian"),
        new("宋体", "SimSun"),
        new("黑体", "SimHei"),
        new("楷体", "KaiTi"),
        new("Segoe UI", "Segoe UI"),
    ];

    private static readonly IReadOnlyList<int> ScaleOptions = [100, 110, 125, 150];

    private readonly IAppSettingsService _settings;

    public AppSettings? Result { get; private set; }

    public SettingsWindow(IAppSettingsService settings)
    {
        InitializeComponent();
        _settings = settings;

        FontComboBox.ItemsSource = Fonts;
        FontComboBox.SelectedValue = _settings.Current.FontFamily;
        ScaleComboBox.ItemsSource = ScaleOptions;
        ScaleComboBox.SelectedValue = _settings.Current.ScalePercent;
    }

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        var fontName = FontComboBox.SelectedValue as string;
        var scaleText = ScaleComboBox.SelectedValue?.ToString();
        if (!int.TryParse(scaleText, out var scalePercent))
        {
            return;
        }

        var settings = new AppSettings
        {
            FontFamily = string.IsNullOrWhiteSpace(fontName) ? _settings.Current.FontFamily : fontName,
            ScalePercent = scalePercent
        };

        _settings.Save(settings);
        Result = settings;
        DialogResult = true;
    }

    /// <summary>
    /// Font choice shown in the settings dialog; ToString is the friendly label
    /// (used by UIA and screen readers).
    /// </summary>
    public sealed record FontOption(string DisplayName, string FontName)
    {
        public override string ToString() => DisplayName;
    }

    private void OnCancelClicked(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
