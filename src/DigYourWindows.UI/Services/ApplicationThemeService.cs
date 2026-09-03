using Wpf.Ui.Appearance;

namespace DigYourWindows.UI.Services;

/// <summary>
/// Abstracts WPF-UI theme application so ViewModels do not touch the
/// framework's static manager directly.
/// </summary>
public interface IApplicationThemeService
{
    ApplicationTheme CurrentTheme { get; }

    event Action<ApplicationTheme>? ThemeChanged;

    /// <summary>
    /// Applies Windows system theme (Dark or Light) and returns the applied theme.
    /// </summary>
    ApplicationTheme ApplySystemTheme();

    /// <summary>
    /// Switches between dark and light, applies the new theme and returns it.
    /// </summary>
    ApplicationTheme ToggleTheme();

    /// <summary>
    /// Applies the specified theme directly.
    /// </summary>
    ApplicationTheme SetTheme(ApplicationTheme theme);
}

public sealed class ApplicationThemeService : IApplicationThemeService
{
    public ApplicationTheme CurrentTheme { get; private set; }

    public event Action<ApplicationTheme>? ThemeChanged;

    public ApplicationThemeService()
    {
        CurrentTheme = ApplicationThemeManager.GetAppTheme();
        ApplicationThemeManager.Changed += OnThemeChanged;
    }

    private void OnThemeChanged(ApplicationTheme currentApplicationTheme, System.Windows.Media.Color systemAccent)
    {
        CurrentTheme = currentApplicationTheme;
        ThemeChanged?.Invoke(CurrentTheme);
    }

    public ApplicationTheme ApplySystemTheme()
    {
        ApplicationThemeManager.ApplySystemTheme();
        CurrentTheme = ApplicationThemeManager.GetAppTheme();
        ThemeChanged?.Invoke(CurrentTheme);
        return CurrentTheme;
    }

    public ApplicationTheme ToggleTheme()
    {
        CurrentTheme = CurrentTheme == ApplicationTheme.Dark
            ? ApplicationTheme.Light
            : ApplicationTheme.Dark;

        ApplicationThemeManager.Apply(CurrentTheme);
        ThemeChanged?.Invoke(CurrentTheme);
        return CurrentTheme;
    }

    public ApplicationTheme SetTheme(ApplicationTheme theme)
    {
        CurrentTheme = theme;
        ApplicationThemeManager.Apply(CurrentTheme);
        ThemeChanged?.Invoke(CurrentTheme);
        return CurrentTheme;
    }
}
