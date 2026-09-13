using System.Collections.Generic;
using System.Windows;
using DigYourWindows.UI.Services;
using Wpf.Ui.Controls;

namespace DigYourWindows.UI.Views.Windows;

/// <summary>
/// Settings dialog: separate English/Chinese font families and UI scale.
/// Returns the chosen settings via <see cref="Result"/> when confirmed.
/// </summary>
public partial class SettingsWindow : FluentWindow
{
    private static readonly IReadOnlyList<FontOption> EnglishFonts =
    [
        new("系统默认（跟随 Windows）", AppSettings.SystemFontFamily),
        new("Segoe UI", "Segoe UI"),
        new("Arial", "Arial"),
        new("Calibri", "Calibri"),
        new("Consolas（等宽）", "Consolas"),
        new("Cascadia Code（等宽）", "Cascadia Code"),
        new("Verdana", "Verdana"),
        new("Times New Roman", "Times New Roman"),
    ];

    private static readonly IReadOnlyList<FontOption> ChineseFonts =
    [
        new("系统默认（跟随 Windows）", AppSettings.SystemFontFamily),
        new("微软雅黑", "Microsoft YaHei UI"),
        new("等线", "DengXian"),
        new("宋体", "SimSun"),
        new("黑体", "SimHei"),
        new("楷体", "KaiTi"),
        new("仿宋", "FangSong"),
    ];

    private static readonly IReadOnlyList<int> ScaleOptions = [100, 110, 125, 150];

    private readonly IAppSettingsService _settings;

    public AppSettings? Result { get; private set; }

    public SettingsWindow(IAppSettingsService settings)
    {
        InitializeComponent();
        _settings = settings;

        EnglishFontComboBox.ItemsSource = EnglishFonts;
        EnglishFontComboBox.SelectedValue = EffectiveSelection(settings.Current.EnglishFontFamily, EnglishFonts);
        ChineseFontComboBox.ItemsSource = ChineseFonts;
        ChineseFontComboBox.SelectedValue = EffectiveSelection(settings.Current.ChineseFontFamily, ChineseFonts);
        ScaleComboBox.ItemsSource = ScaleOptions;
        ScaleComboBox.SelectedValue = settings.Current.ScalePercent;
    }

    /// <summary>
    /// Map a persisted font to the matching dropdown option; system-default chain
    /// (empty) or a font name that is not among the options (e.g. a hand-edited or
    /// uninstalled font) maps to the "follow Windows" entry so the picker never
    /// shows a blank selection.
    /// </summary>
    private static string EffectiveSelection(string font, IReadOnlyList<FontOption> options)
    {
        if (string.IsNullOrWhiteSpace(font) || font == AppSettings.SystemFontFamily)
        {
            return AppSettings.SystemFontFamily;
        }

        return options.Any(o => o.FontName == font) ? font : AppSettings.SystemFontFamily;
    }

    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        var english = EnglishFontComboBox.SelectedValue as string;
        var chinese = ChineseFontComboBox.SelectedValue as string;
        if (!int.TryParse(ScaleComboBox.SelectedValue?.ToString(), out var scalePercent))
        {
            return;
        }

        var settings = new AppSettings
        {
            // "系统默认" maps to the system chain marker; empty strings mean follow system.
            EnglishFontFamily = english == AppSettings.SystemFontFamily ? string.Empty : english ?? string.Empty,
            ChineseFontFamily = chinese == AppSettings.SystemFontFamily ? string.Empty : chinese ?? string.Empty,
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
