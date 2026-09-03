using System.IO;
using System.Text.Json;

namespace DigYourWindows.UI.Services;

/// <summary>
/// User-editable application preferences. Persisted as JSON under %APPDATA%.
/// </summary>
public sealed record AppSettings
{
    /// <summary>UI font family name (a system-installed font).</summary>
    public string FontFamily { get; init; } = "Microsoft YaHei UI";

    /// <summary>UI scale as a percentage: 100 = no scaling.</summary>
    public int ScalePercent { get; init; } = 100;
}

/// <summary>
/// Loads and saves application settings from %APPDATA%\DigYourWindows\settings.json.
/// Missing or corrupt settings fall back to defaults (never throws).
/// </summary>
public interface IAppSettingsService
{
    AppSettings Current { get; }

    void Save(AppSettings settings);
}

public sealed class AppSettingsService : IAppSettingsService
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _settingsPath;
    private readonly object _lock = new();

    public AppSettings Current { get; private set; }

    public AppSettingsService() : this(settingsDirectory: null)
    {
    }

    /// <summary>
    /// Creates the settings store. Pass <paramref name="settingsDirectory"/> to redirect
    /// the settings file (used by tests); null defaults to %APPDATA%\DigYourWindows.
    /// </summary>
    public AppSettingsService(string? settingsDirectory)
    {
        var dir = settingsDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DigYourWindows");
        Directory.CreateDirectory(dir);
        _settingsPath = Path.Combine(dir, "settings.json");

        Current = Load();
    }

    public void Save(AppSettings settings)
    {
        lock (_lock)
        {
            try
            {
                File.WriteAllText(_settingsPath, JsonSerializer.Serialize(settings, JsonOptions));
                Current = settings;
            }
            catch (IOException)
            {
                // Settings persistence is best-effort; never crash the app.
            }
            catch (UnauthorizedAccessException)
            {
                // Settings persistence is best-effort; never crash the app.
            }
        }
    }

    private AppSettings Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                if (settings is not null && IsValid(settings))
                {
                    return settings;
                }
            }
        }
        catch (IOException)
        {
            // Fall back to defaults on unreadable settings.
        }
        catch (UnauthorizedAccessException)
        {
            // Fall back to defaults on unreadable settings.
        }
        catch (JsonException)
        {
            // Fall back to defaults on corrupt settings.
        }

        return new AppSettings();
    }

    private static bool IsValid(AppSettings settings)
    {
        return !string.IsNullOrWhiteSpace(settings.FontFamily) &&
               settings.ScalePercent is >= 80 and <= 250;
    }
}
