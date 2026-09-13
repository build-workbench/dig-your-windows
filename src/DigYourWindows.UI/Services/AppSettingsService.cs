using System.IO;
using System.Text.Json;

namespace DigYourWindows.UI.Services;

/// <summary>
/// User-editable application preferences. Persisted as JSON under %APPDATA%.
/// </summary>
public sealed record AppSettings
{
    /// <summary>
    /// The "follow Windows" font chain: Segoe UI Variable on Windows 11,
    /// falling back to Microsoft YaHei UI for CJK and Segoe UI elsewhere.
    /// </summary>
    public const string SystemFontFamily = "Segoe UI Variable Text, Microsoft YaHei UI, Segoe UI";

    /// <summary>
    /// Font for Latin/Greek/Cyrillic text. Empty = follow the system default chain.
    /// </summary>
    public string EnglishFontFamily { get; init; } = string.Empty;

    /// <summary>
    /// Font for CJK text (Chinese characters and CJK punctuation).
    /// Empty = follow the system default chain.
    /// </summary>
    public string ChineseFontFamily { get; init; } = string.Empty;

    /// <summary>UI scale as a percentage: 100 = no scaling.</summary>
    public int ScalePercent { get; init; } = 100;

    /// <summary>
    /// Legacy single-font settings stored the chosen font in <see cref="FontFamily"/>.
    /// Migrate it into the Chinese slot (the legacy value was typically a CJK-capable
    /// font), then clear it so migration happens exactly once.
    /// </summary>
    public AppSettings MigrateLegacyFontFamily()
    {
        if (string.IsNullOrEmpty(FontFamily))
        {
            return this;
        }

        var legacy = FontFamily;
        return this with
        {
            ChineseFontFamily = legacy == SystemFontFamily ? string.Empty : legacy,
            FontFamily = string.Empty
        };
    }

    /// <summary>Legacy single-font field kept only for settings-file migration.</summary>
    public string FontFamily { get; init; } = string.Empty;
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
                    return settings.MigrateLegacyFontFamily();
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
        return settings.ScalePercent is >= 80 and <= 250;
    }
}
