using Wpf.Ui.Appearance;

namespace DigYourWindows.UI.Services;

/// <summary>
/// Abstracts WPF-UI theme application so ViewModels do not touch the
/// framework's static manager directly.
/// </summary>
public interface IApplicationThemeService
{
    ApplicationTheme CurrentTheme { get; }

    /// <summary>
    /// Switches between dark and light, applies the new theme and returns it.
    /// </summary>
    ApplicationTheme ToggleTheme();
}

public sealed class ApplicationThemeService : IApplicationThemeService
{
    public ApplicationTheme CurrentTheme { get; private set; } = ApplicationTheme.Dark;

    public ApplicationTheme ToggleTheme()
    {
        CurrentTheme = CurrentTheme == ApplicationTheme.Dark
            ? ApplicationTheme.Light
            : ApplicationTheme.Dark;

        ApplicationThemeManager.Apply(CurrentTheme);
        return CurrentTheme;
    }
}
